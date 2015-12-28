namespace GUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLogin = new System.Windows.Forms.Button();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnBackUpPictures = new System.Windows.Forms.Button();
            this.picBoxLoading = new System.Windows.Forms.PictureBox();
            this.progBarDownload = new System.Windows.Forms.ProgressBar();
            this.lblPictureProgress = new System.Windows.Forms.Label();
            this.btnLogOut = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLoading)).BeginInit();
            this.SuspendLayout();
            // 
            // btnLogin
            // 
            this.btnLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogin.Location = new System.Drawing.Point(544, 56);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(41, 20);
            this.btnLogin.TabIndex = 3;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(411, 56);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(127, 20);
            this.txtPassword.TabIndex = 1;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(271, 56);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(127, 20);
            this.txtUsername.TabIndex = 0;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.ForeColor = System.Drawing.Color.White;
            this.lblUserName.Location = new System.Drawing.Point(268, 40);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(32, 13);
            this.lblUserName.TabIndex = 3;
            this.lblUserName.Text = "Email";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.ForeColor = System.Drawing.Color.White;
            this.lblPassword.Location = new System.Drawing.Point(408, 40);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "Password";
            // 
            // btnBackUpPictures
            // 
            this.btnBackUpPictures.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.btnBackUpPictures.Location = new System.Drawing.Point(455, 133);
            this.btnBackUpPictures.Name = "btnBackUpPictures";
            this.btnBackUpPictures.Size = new System.Drawing.Size(130, 23);
            this.btnBackUpPictures.TabIndex = 7;
            this.btnBackUpPictures.Text = "Backup My Pictures";
            this.btnBackUpPictures.UseVisualStyleBackColor = true;
            this.btnBackUpPictures.Visible = false;
            this.btnBackUpPictures.Click += new System.EventHandler(this.btnBackUpPictures_Click);
            // 
            // picBoxLoading
            // 
            this.picBoxLoading.Image = global::GUI.Properties.Resources.Loading;
            this.picBoxLoading.Location = new System.Drawing.Point(591, 56);
            this.picBoxLoading.Name = "picBoxLoading";
            this.picBoxLoading.Size = new System.Drawing.Size(35, 23);
            this.picBoxLoading.TabIndex = 10;
            this.picBoxLoading.TabStop = false;
            this.picBoxLoading.Visible = false;
            // 
            // progBarDownload
            // 
            this.progBarDownload.Location = new System.Drawing.Point(271, 101);
            this.progBarDownload.Name = "progBarDownload";
            this.progBarDownload.Size = new System.Drawing.Size(314, 23);
            this.progBarDownload.TabIndex = 11;
            this.progBarDownload.Visible = false;
            // 
            // lblPictureProgress
            // 
            this.lblPictureProgress.AutoSize = true;
            this.lblPictureProgress.ForeColor = System.Drawing.Color.White;
            this.lblPictureProgress.Location = new System.Drawing.Point(268, 85);
            this.lblPictureProgress.Name = "lblPictureProgress";
            this.lblPictureProgress.Size = new System.Drawing.Size(48, 13);
            this.lblPictureProgress.TabIndex = 3;
            this.lblPictureProgress.Text = "Pictures:";
            this.lblPictureProgress.Visible = false;
            // 
            // btnLogOut
            // 
            this.btnLogOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogOut.Location = new System.Drawing.Point(544, 56);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(41, 20);
            this.btnLogOut.TabIndex = 3;
            this.btnLogOut.Text = "Logout";
            this.btnLogOut.UseVisualStyleBackColor = true;
            this.btnLogOut.Visible = false;
            this.btnLogOut.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(54, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 33);
            this.label1.TabIndex = 12;
            this.label1.Text = "f Photo Backup";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(652, 188);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progBarDownload);
            this.Controls.Add(this.picBoxLoading);
            this.Controls.Add(this.btnBackUpPictures);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblPictureProgress);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnLogOut);
            this.Controls.Add(this.btnLogin);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.picBoxLoading)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnBackUpPictures;
        private System.Windows.Forms.PictureBox picBoxLoading;
        private System.Windows.Forms.ProgressBar progBarDownload;
        private System.Windows.Forms.Label lblPictureProgress;
        private System.Windows.Forms.Button btnLogOut;
        private System.Windows.Forms.Label label1;
    }
}

