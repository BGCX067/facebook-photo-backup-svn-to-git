using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using System.Collections;

namespace FacebookPhotoGrabber
{
    public struct UserLoggedEventArgs
    {
        public string _userName;
        public string _password;
        public int _number_of_pictures;

        public UserLoggedEventArgs(string userName, string password, int number_of_pictures)
        {
            _userName = userName;
            _password = password;
            _number_of_pictures = number_of_pictures;
        }
    }

    public struct DownloadedPicture // Currently doesn't help much but might be extended in the future.
    {
        private Uri _picture_uri;

        public DownloadedPicture(Uri picture_uri)
        {
            _picture_uri = picture_uri;
        }
    }

    /// <summary>
    /// FacebookAPI contains the basic API to login and find / download photos.
    /// Uses Facebook's mobile interface (m.facebook.com) instead of the "regular" version, to reduce both network traffic and network access. 
    /// </summary>
    public class FacebookAPI
    {
        const string facebook_base_url = "http://m.facebook.com";
        const string facebook_logged_title = "<title>Home</title>";
        const string facebook_image_ext = ".jpg";
        const int number_of_pictures_in_page = 10;  // How many pictures have we got on a single page.

        HTTP http;                          // All HTTP communication is performed via this class. 
        Uri facebook_home_page = new Uri(facebook_base_url);
        Uri facebook_logout_page = null;
        Uri facebook_photos_page = null;
        Uri facebook_profile_page = null;

        HtmlDocument main_page;
        HtmlDocument profile_page;
        HtmlDocument pictures_page;

        int users_id;                       // User ID on facebook.
        int number_of_photos;               // How many pictures are there.
        int number_of_photos_downloaded;    // How many pictures were downloaded.
        int next_page_index;
        int prev_page_index;
        string folderPath;                  // Where to save photos.

        Queue<Uri> pictures_urls;           // Queue of photo URLs to be downloaded.
        List<Thread> consumers;             // List of threads which download photos.
        Thread producer_one;                // One of the threads responsible to fill up the queue with photo URLs.
        Thread producer_two;                // One of the threads responsible to fill up the queue with photo URLs.


        const int number_of_worker_threads = 20;
        Mutex mutex;
        Semaphore semaphore;

        private delegate int delegate_GetNexPrevPictureUrl();
        public delegate void delegate_UserLoggedEventHandler(object sender, UserLoggedEventArgs e);
        public delegate void delegate_PicrureDownloaded(object sender, DownloadedPicture e);

        public event delegate_UserLoggedEventHandler UserLogged;
        public event delegate_PicrureDownloaded PictureDownloaded;

        public string FolderPath
        {
            get
            {
                return folderPath;
            }
            set
            {
                folderPath = value;
            }
        }

        #region Ctor

        public FacebookAPI()
        {
            http = new HTTP();

            // Reset locals.
            number_of_photos = 0;
            number_of_photos_downloaded = 0;
        }

        #endregion

        #region Destructor
        ~FacebookAPI()
        {
            // Kill all open threads.
            KillAllThreads();
        }

        #endregion

        /// <summary>
        /// Logs in to facebook.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool Login(string username, string password)
        {
            // Send POST message to Facebook homepage.
            string response = http.GET(facebook_home_page); // Getting some cookies from Facebook.
            HtmlDocument loginPage = new HtmlDocument();
            loginPage.LoadHtml(response);

            // Find out where to send the user's credentials.
            HtmlNodeCollection form = loginPage.DocumentNode.SelectNodes("//form");
            if (form.Count != 1)
                throw new Exception("Can't find form element");

            string action = form[0].GetAttributeValue("action", "");

            Dictionary<string, string> login_params = new Dictionary<string, string>();

            // Add credentials as parameters.            
            HtmlNodeCollection inputs = loginPage.DocumentNode.SelectNodes("//input");
            if (inputs.Count == 0)
                throw new Exception("Can't find form element");

            foreach (HtmlNode input in inputs)
            {
                login_params.Add(input.GetAttributeValue("name", ""), input.GetAttributeValue("value", ""));
            }
            login_params["email"] = username;
            login_params["pass"] = password;

            // Send credentials to perform login.
            response = http.POST(action, login_params);

            // See if we're indeed logged in?
            if (!response.Contains(facebook_logged_title))
                return false;

            // Now that we're logged in, get user's profile page and determine how many pictures the user have. 
            main_page = new HtmlDocument();
            main_page.LoadHtml(response);

            // Get logout url.
            List<HtmlNode> logout_link = FindElementByInnerText(main_page.DocumentNode.SelectNodes("//a"), "Logout");
            if (logout_link.Count == 1)
                facebook_logout_page = new Uri(GetLinkAbsHREF(logout_link[0]));

            if (!GetProfilePage())
                return false;

            if (!GetPhotosPage(out number_of_photos))
                return false;

            if (UserLogged != null)
            {
                UserLogged(this, new UserLoggedEventArgs(username, password, number_of_photos));
            }

            CreatePhotoUrlsFetchers();

            return true;
        }

        // Logs out of Facebook.
        // TODO: Check and Call.
        public bool Logout()
        {
            if (facebook_logout_page != null)
            {
                http.GET(facebook_logout_page);

                // Clear cookies
                http.ClearCookies();
            }

            // TODO: Make sure we're logged out.
            return true;
        }

        /// <summary>
        /// Retrieves user's profile page.
        /// </summary>
        /// <returns></returns>
        public bool GetProfilePage()
        {
            // Search for Profile page link
            List<HtmlNode> findings = FindElementByInnerText(main_page.DocumentNode.SelectNodes("//a"), "Profile");
            if (findings.Count == 0)
                return false;

            HtmlNode link = findings[0];
            string href = GetLinkAbsHREF(link);

            if (href == string.Empty)
            {
                return false;
            }

            facebook_profile_page = new Uri(href);
            string response = http.GET(facebook_profile_page);

            profile_page = new HtmlDocument();
            profile_page.LoadHtml(response);

            // TODO: Check we've managed to get the profile page.
            return true;
        }

        /// <summary>
        /// Sets the pictures_page HTMLDocument used later by GetPhotosPage.
        /// </summary>
        /// <returns></returns>
        private bool GetPhotoAlbumsPage()
        {
            if (profile_page == null)
                return false;

            // Find link to photo albums.
            List<HtmlNode> findings = FindElementByInnerText(profile_page.DocumentNode.SelectNodes("//a"), "Photos");
            if (findings.Count == 0)
                return false;

            HtmlNode link = findings[0];

            string href = GetLinkAbsHREF(link);
            facebook_photos_page = new Uri(href);
            string response = http.GET(facebook_photos_page);

            pictures_page = new HtmlDocument();

            pictures_page.LoadHtml(response);

            return true;
        }

        /// <summary>        
        /// GetPhotosPage Gets a link to the first photo page of "Photos of You" URL.
        /// Gets the number of pictures in the album.
        /// Also set the user's ID.
        /// </summary>
        /// <param name="album_url"></param>
        /// <param name="number_of_pictures"></param>
        /// <param name="last_page_index"></param>
        /// <returns></returns>
        public bool GetPhotosPage(out int out_number_of_pictures)
        {
            out_number_of_pictures = 0;

            if (GetPhotoAlbumsPage() == false)
                return false;

            // Locate Added by Others show all link.
            List<HtmlNode> findings = FindElementByInnerText(pictures_page.DocumentNode.SelectNodes("//a"), "Photos of You");
            if (findings.Count == 0)
            {
                // No pictures.
                return false;
            }

            HtmlNode link = findings[0]; // Get the last link.
            string href = GetLinkAbsHREF(link);
            facebook_photos_page = new Uri(href);   // Overwrites this member set previously by GetPhotoAlbumsPage. Might want to change it.

            string number_of_pictures = link.NextSibling.NextSibling.NextSibling.InnerText.Replace(" photos", "");
            if (!int.TryParse(number_of_pictures, out out_number_of_pictures))
                return false;

            // Break down photo album url. http://m.facebook.com/media/set/?set=t.687433935&op=1&v&refid=0
            // Remove unnecessary parametrs.

            // We're interested in the "set" param.
            Dictionary<string, string> albumUrlParams = new Dictionary<string, string>();
            string query = facebook_photos_page.Query;
            string[] queryParams = query.Split(new char[1] { '&' });
            if (queryParams.Length == 0)
                return false;

            queryParams[0] = queryParams[0].Substring(1, queryParams[0].Length - 1); // Removes '?'.
            foreach (string s in queryParams)
            {
                string[] keyValue = s.Split(new char[1] { '=' });
                string key = keyValue[0];
                string value = keyValue[1];

                if (key == "set")
                {
                    albumUrlParams.Add(key, value);
                    users_id = int.Parse(value.Replace("t.", "")); // Sets user's ID. //TODO: remove all non-numeric chars.
                    break;
                }
            }

            string album_url = "http://m.facebook.com" + facebook_photos_page.AbsolutePath + "?";
            foreach (string key in albumUrlParams.Keys)
            {
                album_url += key + "=" + albumUrlParams[key] + "&";
            }
            album_url = album_url.Substring(0, album_url.Length - 1);

            facebook_photos_page = new Uri(album_url);

            return true;
        }

        /// <summary>
        /// Creats a thread which will fetch photos urls and download them.
        /// </summary>
        public void BackUpPictures()
        {   
            consumers = new List<Thread>(number_of_worker_threads); // These threads will download photos from the queue.
            for (int thread_count = 0; thread_count < number_of_worker_threads; thread_count++)
            {
                Thread consumer = new Thread(new ThreadStart(Worker));
                consumers.Add(consumer);
                consumer.Start();
            }
        }

        /// <summary>
        /// Creats two threads which will fill up our queue with urls to photos. 
        /// </summary>
        private void CreatePhotoUrlsFetchers()
        {
            pictures_urls = new Queue<Uri>(number_of_photos);

            next_page_index = number_of_photos / 2; // middle.
            next_page_index -= (next_page_index % number_of_pictures_in_page); // make sure we're a multiple of number_of_pictures_in_page.

            prev_page_index = next_page_index;  // BuildPicturesList method will remove number_of_pictures_in_page from prev_page_index as it starts. 
            next_page_index -= number_of_pictures_in_page;  // BuildPicturesList method will add number_of_pictures_in_page to next_page_index as it starts.

            mutex = new Mutex();
            semaphore = new Semaphore(0, 600); // Should be enough.

            // Create two threads which will enter photos urls to queue.

            producer_one = new Thread(new ParameterizedThreadStart(BuildPicturesList));
            delegate_GetNexPrevPictureUrl next_method = new delegate_GetNexPrevPictureUrl(GetNextPicturesPageIndex);
            producer_one.Start(next_method);

            producer_two = new Thread(new ParameterizedThreadStart(BuildPicturesList));
            delegate_GetNexPrevPictureUrl prev_method = new delegate_GetNexPrevPictureUrl(GetPrevPicturesPageIndex);
            producer_two.Start(prev_method);
        }

        /// <summary>
        /// Adds picture URLs to queue, At the moment two threads are scanning the user's "Photos of You" album,
        /// both starts from the 'middle' of the album one advances forward the other backwards,
        /// this way the queue will fill up much quicker.
        /// </summary>
        /// <param name="next_prev_pic_page_url">Method which navigates to a photos page.</param>
        private void BuildPicturesList(object next_prev_pic_page_url)
        {
            try
            {
                // Casting given parameter.
                delegate_GetNexPrevPictureUrl getNexPrevPictureUrl = next_prev_pic_page_url as delegate_GetNexPrevPictureUrl;

                List<string> pictures_srcs; // list of picture URLs for the current page.
                int current_page_index = getNexPrevPictureUrl(); // Starting position. we navigate both forward and backwards.

                do
                {
                    pictures_srcs = GetPicturedSrcs(current_page_index); // Get all photo URLs in current page.
                    if (pictures_srcs.Count == 0)
                        break;

                    mutex.WaitOne(); // TODD: check if queue is thread safe, if so mutex isn't needed.

                    foreach (string picture_src in pictures_srcs)
                        pictures_urls.Enqueue(new Uri(picture_src));

                    mutex.ReleaseMutex();

                    semaphore.Release(pictures_srcs.Count); // one for each image.

                    current_page_index = getNexPrevPictureUrl(); // Either move forward or backwards depending on the given next_prev_pic_page_url param.

                } while (pictures_srcs.Count > 0 && current_page_index >= 0); // index must be positive or equal to zero and we must have some pictures.
            }
            catch (ThreadAbortException ex)
            {
            }

            // We can logout once the queue is filled by both threads, Facebook doesn't check credentials when acssesing a photo directly.
            // But for the time being stay logged.
        }

        /// <summary>
        /// Moves photo page index forward by number_of_pictures_in_page.
        /// </summary>
        /// <returns></returns>
        private int GetNextPicturesPageIndex()
        {
            next_page_index += number_of_pictures_in_page;
            return next_page_index;
        }

        /// <summary>
        /// Moves photo page index backwards by number_of_pictures_in_page.
        /// </summary>
        /// <returns></returns>
        private int GetPrevPicturesPageIndex()
        {
            prev_page_index -= number_of_pictures_in_page;
            return prev_page_index;
        }

        /// <summary>
        ///  Gets all picture URLs under the given photo page index.
        /// </summary>
        /// <param name="page_index"></param>
        /// <returns></returns>
        private List<string> GetPicturedSrcs(int page_index)
        {
            List<string> urls = new List<string>(number_of_pictures_in_page);

            string url = facebook_photos_page.ToString();
            url += "&s=" + page_index;  //http://m.facebook.com/media/set/?set=t.user'sID&s=10


            string response = http.GET(new Uri(url));
            HtmlDocument pictures_page = new HtmlDocument();
            pictures_page.LoadHtml(response);

            HtmlNodeCollection images = pictures_page.DocumentNode.SelectNodes("//img");
            for (int image_index = 1; image_index < images.Count; image_index++) // Skip facebook logo.
            {
                HtmlNode img = images[image_index];
                string src = img.GetAttributeValue("src", "");

                if (!src.Contains(facebook_image_ext)) // This is not an image.
                    continue;

                src = src.Replace("_t", "_n");
                src = src.Replace("_a", "_n");
                src = src.Replace("/t", "/n");
                src = src.Replace("/a", "/n");
                urls.Add(src);
            }

            return urls;
        }

        /// <summary>
        /// This is the worker thread's method,
        /// Its task is to fetch picture URLs from the queue and download them.
        /// </summary>
        private void Worker()
        {
            try
            {
                while (number_of_photos_downloaded < number_of_photos) // It seems facebook some times claims a user have more phtos in his album then he/she actually has.
                //while (pictures_urls.Count > 0)
                {
                    semaphore.WaitOne();    // Wait for Producer to add Url to the list.                
                    mutex.WaitOne();
                    Uri picture_url = pictures_urls.Dequeue();
                    mutex.ReleaseMutex();

                    DownloadPicture(picture_url); // No, the thread doesn't release our semaphore...

                    if (PictureDownloaded != null)
                    {
                        PictureDownloaded(this, new DownloadedPicture(picture_url));
                    }
                }
            }
            catch (ThreadAbortException ex)
            {

            }
        }

        /// <summary>
        /// Downloads the given url (picture) to the selected folder.
        /// </summary>
        /// <param name="picture_url"></param>
        public void DownloadPicture(Uri picture_url)
        {
            // Download picture.
            MemoryStream data = http.DOWNLOAD(picture_url);

            // Get file name.
            string file_name = picture_url.AbsoluteUri.ToString();
            file_name = file_name.Substring(file_name.LastIndexOf('/') + 1, file_name.Length - file_name.LastIndexOf('/') - 1);

            FileStream fs = new FileStream(folderPath + "\\" + file_name, FileMode.Create);

            int read_bytes = 0;
            byte[] buffer = new byte[8192];
            while ((read_bytes = data.Read(buffer, 0, 8192)) > 0)
            {
                fs.Write(buffer, 0, read_bytes); // Write data to file.
            }

            fs.Close();
            data.Close();

            number_of_photos_downloaded++;
        }

        /// <summary>
        /// Converts a relative link to an absolut url.
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private string GetLinkAbsHREF(HtmlNode link)
        {
            string href = link.GetAttributeValue("href", "");
            if (!href.StartsWith(facebook_base_url)) // Is it a relative address?
            {
                if (href[0] != '/')
                {
                    // Abs url.
                    href = '/'+ href;
                }
                href = facebook_base_url + href;
            }

            return href;
        }

        /// <summary>
        /// Searches given elements parameter for subset of elements with the same inner text as given in the innerText parameter.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="innerText"></param>
        /// <returns></returns>
        private List<HtmlNode> FindElementByInnerText(HtmlNodeCollection elements, string innerText)
        {
            List<HtmlNode> findings = new List<HtmlNode>(elements.Count);

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].InnerText == innerText)
                {
                    findings.Add(elements[i]);
                }
            }
            return findings;
        }

        /// <summary>
        /// Aborts all threads created.
        /// </summary>
        public void KillAllThreads()
        {
            if (consumers != null)
            {
                foreach (Thread t in consumers)
                {
                    t.Abort();
                }

                consumers = null;
            }

            if (producer_one != null)
            {
                producer_one.Abort();
                producer_one = null;
            }

            if (producer_two != null)
            {
                producer_two.Abort();
                producer_two = null;
            }
        }
    }
}