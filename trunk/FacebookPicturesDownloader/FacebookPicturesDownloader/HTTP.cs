using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.IO;

namespace FacebookPhotoGrabber
{
    public enum enumWebRequestType
    {
        GET,
        POST,
    }

    /// <summary>
    /// HTTP suposed to simplified the way we make an http request (POST / GET).
    /// Desn't suports unicode.
    /// </summary>
    public class HTTP
    {
        // Mobile user agent.
        string USER_AGENT = "Opera/9.51 Beta (Microsoft Windows; PPC; Opera Mobi/1718; U; en)";

        CookieContainer cookies;    // Holds cookies recived while using an instance of the HTTP class, all HttpWebRequest object will use the same cookie container.

        #region Ctor

        public HTTP()
        {
            cookies = new CookieContainer(); // Init cookies jar.
        }

        #endregion

        /// <summary>
        /// Sends a GET request.
        /// </summary>
        /// <param name="address">Request's address</param>
        /// <returns>Response</returns>
        public string GET(Uri address)
        {
            // No additional parameters.
            return GET(address, null);
        }

        /// <summary>
        /// Sends a GET request.
        /// </summary>
        /// <param name="address">Request's address</param>
        /// <param name="get_params">Request's GET parameters</param>
        /// <returns>Response</returns>
        public string GET(Uri address, Dictionary<string, string> get_params)
        {
            string strAddress = address.ToString();

            // Handle parameters, Adds given parameters to request address.
            if (get_params != null && get_params.Count > 0)
            {
                StringBuilder concatedParams = new StringBuilder();
                concatedParams.Append("?");

                foreach (string key in get_params.Keys)
                {
                    string value = get_params[key];
                    concatedParams.Append(key);
                    concatedParams.Append("=");
                    concatedParams.Append(value);
                    concatedParams.Append("&");
                }
                concatedParams.Remove(concatedParams.Length - 1, 1);
                strAddress += concatedParams;
            }

            HttpWebRequest webRequest = GetRequestObject(enumWebRequestType.GET, strAddress); // Gets a new request object.

            StreamReader sr = null;
            WebResponse webResponse = null;
            string response = "";
            try // get the response
            {
                webResponse = webRequest.GetResponse();
                sr = new StreamReader(webResponse.GetResponseStream());
                response = sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
                // TODO: handle exception.
            }
            finally
            {
                if (webResponse != null)
                    webResponse.Close();

                if (sr != null)
                    sr.Close();
            }

            response = HttpUtility.HtmlDecode(response);
            return response;
        }

        /// <summary>
        /// Sends a POST request.
        /// </summary>
        /// <param name="address">Request's address</param>
        /// <param name="post_params">Request's POST parameters, currently doesn't suports files (Multi-Part).</param>
        /// <returns>Response</returns>
        public string POST(string address, Dictionary<string, string> post_params)
        {
            return POST(new Uri(address), post_params);
        }

        /// <summary>
        /// Sends a POST request.
        /// </summary>
        /// <param name="address">Request's address</param>
        /// <param name="post_params">Request's POST parameters, currently doesn't suports files (Multi-Part).</param>
        /// <returns>Response</returns>
        public string POST(Uri address, Dictionary<string, string> post_params)
        {
            HttpWebRequest webRequest = GetRequestObject(enumWebRequestType.POST, address);

            // Handle parameters, Adds given parameters to request address.
            StringBuilder concatedParams = new StringBuilder(); 
            foreach (string key in post_params.Keys)
            {
                string value = post_params[key];
                concatedParams.Append(key);
                concatedParams.Append("=");
                concatedParams.Append(value);
                concatedParams.Append("&");
            }

            concatedParams.Remove(concatedParams.Length - 1, 1);
            byte[] data = Encoding.ASCII.GetBytes(concatedParams.ToString());

            webRequest.ContentLength = data.Length;

            Stream stream = null;
            try // Sends request
            {
                stream = webRequest.GetRequestStream();
                stream.Write(data, 0, data.Length);
            }
            catch (WebException ex)
            {
                // TODO: Handle exception.
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            StreamReader sr = null;
            WebResponse webResponse = null;
            string response = "";
            try // Read response.
            {
               webResponse = webRequest.GetResponse();                
               sr = new StreamReader(webResponse.GetResponseStream());
               response = sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
                // TODO: Handle exception.
            }
            finally
            {
                webResponse.Close();
                if(sr != null)
                    sr.Close();
            }

            response = HttpUtility.HtmlDecode(response);
            return response;
        }

        /// <summary>
        /// Downloads a given url. Make sure to close the returned MemoryStream.
        /// </summary>
        /// <param name="address">Resource address</param>
        /// <returns>Resource in memory</returns>
        public MemoryStream DOWNLOAD(Uri address)
        {
            string strAddress = address.ToString();

            HttpWebRequest httpWebRequest = GetRequestObject(enumWebRequestType.GET, address);

            WebResponse webResponse = null;
            MemoryStream ms = new MemoryStream();

            try // Read response.
            {
                webResponse = httpWebRequest.GetResponse();
                if (webResponse == null)
                {
                    int a = 1;
                    a++;
                }
                int bytes_read = 0;
                byte[] data = new byte[8192]; // Temp Buffer.
                while ((bytes_read = webResponse.GetResponseStream().Read(data, 0, 8192)) > 0)
                {
                    ms.Write(data, 0, bytes_read);
                }
                ms.Position = 0; // Reset position.
            }
            catch (WebException ex)
            {
                // TODO: handle exception.
            }
            finally
            {
                if (webResponse != null)
                    webResponse.Close();
            }
            return ms;
        }

        /// <summary>
        /// Clears the current cookie jar by setting it to a new one.
        /// </summary>
        public void ClearCookies()
        {
            cookies = new CookieContainer();
        }

        /// <summary>
        /// Creates a HttpWebRequest objects sets it's cookies jar, UserAgent and Method.
        /// </summary>
        /// <param name="requestType">POST / GET</param>
        /// <param name="address">Request's address</param>
        /// <returns>ready to use HttpWebRequest object</returns>
        private HttpWebRequest GetRequestObject(enumWebRequestType requestType, string address)
        {
            return GetRequestObject(requestType, new Uri(address));
        }

        /// <summary>
        /// Creates a HttpWebRequest objects sets it's cookies jar, UserAgent and Method.
        /// </summary>
        /// <param name="requestType">POST / GET</param>
        /// <param name="address">Request's address</param>
        /// <returns>ready to use HttpWebRequest object</returns>
        private HttpWebRequest GetRequestObject(enumWebRequestType requestType, Uri address)
        {
            HttpWebRequest webRequest = HttpWebRequest.Create(address) as HttpWebRequest;
            webRequest.UserAgent = USER_AGENT;
            webRequest.CookieContainer = cookies;

            if(requestType == enumWebRequestType.POST)
            {
                webRequest.Method = "POST";
            }
            else if(requestType == enumWebRequestType.GET)
            {
                webRequest.Method = "GET";
            }
            else
            {
                throw new Exception("Unknow request type");
            }
            return webRequest;
        }        
    }
}
