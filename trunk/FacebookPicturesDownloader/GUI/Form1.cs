using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FacebookPhotoGrabber;
using System.Threading;

namespace GUI
{
    public partial class Form1 : Form
    {
        FacebookAPI facebook = null;
        private delegate void delegate_facebook_UserLogged(object sender, UserLoggedEventArgs e);
        private delegate void delegate_facebook_PictureDownloaded(object sender, DownloadedPicture e);
        private delegate void delegate_SwapLoginInputs();
        private delegate void delegate_HideLoading();

        public Form1()
        {
            InitializeComponent();
            BackColor = Color.FromArgb(59, 88, 151);
            Font = new Font("lucida grand", 14, GraphicsUnit.Pixel);

            progBarDownload.Minimum = 0;
            progBarDownload.Maximum = 0;
            progBarDownload.Step = 1;

            facebook = new FacebookAPI();
            facebook.UserLogged += new FacebookAPI.delegate_UserLoggedEventHandler(facebook_UserLogged);
            facebook.PictureDownloaded += new FacebookAPI.delegate_PicrureDownloaded(facebook_PictureDownloaded);
        }

        #region Event handlers

        void facebook_UserLogged(object sender, UserLoggedEventArgs e)
        {
            if (InvokeRequired)
            {
                delegate_facebook_UserLogged dlgFacebook_UserLogged = new delegate_facebook_UserLogged(facebook_UserLogged);
                this.Invoke(dlgFacebook_UserLogged, new object[] { sender, e });
            }
            else
            {
                SwapLoginButtons();
                HideLoading();
                SwapProgressBar();

                progBarDownload.Maximum = e._number_of_pictures;
                lblPictureProgress.Text = progBarDownload.Value + "/" + progBarDownload.Maximum;
            }
        }

        private void SwapProgressBar()
        {
            progBarDownload.Visible = !progBarDownload.Visible;
            btnBackUpPictures.Visible = !btnBackUpPictures.Visible;
            lblPictureProgress.Visible = !lblPictureProgress.Visible;
        }

        void facebook_PictureDownloaded(object sender, DownloadedPicture e)
        {
            if (InvokeRequired)
            {
                delegate_facebook_PictureDownloaded dlgFacebook_PictureDownloaded = new delegate_facebook_PictureDownloaded(facebook_PictureDownloaded);
                this.Invoke(dlgFacebook_PictureDownloaded, new object[] { sender, e });
            }
            else
            {
                progBarDownload.PerformStep();
                lblPictureProgress.Text = progBarDownload.Value + "/" + progBarDownload.Maximum;

                if (progBarDownload.Value == progBarDownload.Maximum) // This is not always the case...
                {
                    // Done downloading.
                }
            }
        }

        #endregion

        private void AsyncLogin()
        {
            if (facebook.Login(txtUsername.Text, txtPassword.Text))
            {

            }
            else
            {
                HideLoading();
                SwapLoginInputs();
            }
        }

        private void HideLoading()
        {
            if (InvokeRequired)
            {
                delegate_HideLoading dlgHideLoading = new delegate_HideLoading(HideLoading);
                this.Invoke(dlgHideLoading);
            }
            else
            {
                picBoxLoading.Visible = false;
            }
        }

        private void btnBackUpPictures_Click(object sender, EventArgs e)
        {
            // Ask for path.
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
            folderBrowser.Description = "Where do you want to save your pictures to?";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                facebook.FolderPath = folderBrowser.SelectedPath;
                facebook.BackUpPictures();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            SwapLoginInputs();
            picBoxLoading.Visible = true;

            Thread loginThread = new Thread(new ThreadStart(AsyncLogin));
            loginThread.Start();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (!facebook.Logout())
            {
                // Logout failed.
            }

            SwapLoginInputs();
            SwapLoginButtons();
            SwapProgressBar();
        }

        private void SwapLoginInputs()
        {
            if (InvokeRequired)
            {
                delegate_SwapLoginInputs dlgSwapLoginInputs = new delegate_SwapLoginInputs(SwapLoginInputs);
                this.Invoke(dlgSwapLoginInputs);
            }
            else
            {
                txtPassword.Enabled = !txtPassword.Enabled;
                txtUsername.Enabled = !txtUsername.Enabled;
            }
        }

        private void SwapLoginButtons()
        {
            btnLogin.Visible = !btnLogin.Visible;
            btnLogOut.Visible = !btnLogOut.Visible;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            facebook.KillAllThreads();
        }
    }
}