namespace CrypTalk
{
    partial class Login
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            pictureBox1 = new PictureBox();
            lblLoginForm = new Label();
            btnLogin = new Button();
            llblForgotPassword = new LinkLabel();
            llblRegister = new LinkLabel();
            txtUsernameForm = new Guna.UI2.WinForms.Guna2TextBox();
            txtPasswordForm = new Guna.UI2.WinForms.Guna2TextBox();
            plLoginForm = new Panel();
            label1 = new Label();
            plHeader = new Panel();
            label2 = new Label();
            btnClose = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            plLoginForm.SuspendLayout();
            plHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(831, 487);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // lblLoginForm
            // 
            lblLoginForm.AutoSize = true;
            lblLoginForm.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblLoginForm.ForeColor = Color.White;
            lblLoginForm.Location = new Point(121, 28);
            lblLoginForm.Name = "lblLoginForm";
            lblLoginForm.Size = new Size(75, 31);
            lblLoginForm.TabIndex = 1;
            lblLoginForm.Text = "Login";
            // 
            // btnLogin
            // 
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(20, 209);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(281, 31);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "LOGIN";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            btnLogin.MouseEnter += btnLogin_MouseEnter_1;
            btnLogin.MouseLeave += btnLogin_MouseLeave;
            // 
            // llblForgotPassword
            // 
            llblForgotPassword.AutoSize = true;
            llblForgotPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            llblForgotPassword.LinkColor = Color.White;
            llblForgotPassword.Location = new Point(167, 171);
            llblForgotPassword.Name = "llblForgotPassword";
            llblForgotPassword.Size = new Size(134, 20);
            llblForgotPassword.TabIndex = 6;
            llblForgotPassword.TabStop = true;
            llblForgotPassword.Text = "Forgot password?";
            llblForgotPassword.LinkClicked += llblForgotPassword_LinkClicked;
            // 
            // llblRegister
            // 
            llblRegister.AutoSize = true;
            llblRegister.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            llblRegister.LinkColor = Color.White;
            llblRegister.Location = new Point(195, 243);
            llblRegister.Name = "llblRegister";
            llblRegister.Size = new Size(67, 20);
            llblRegister.TabIndex = 6;
            llblRegister.TabStop = true;
            llblRegister.Text = "Register";
            llblRegister.LinkClicked += llblRegister_LinkClicked_1;
            // 
            // txtUsernameForm
            // 
            txtUsernameForm.BorderRadius = 18;
            txtUsernameForm.CustomizableEdges = customizableEdges5;
            txtUsernameForm.DefaultText = "";
            txtUsernameForm.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtUsernameForm.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtUsernameForm.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtUsernameForm.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtUsernameForm.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtUsernameForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtUsernameForm.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtUsernameForm.IconRight = (Image)resources.GetObject("txtUsernameForm.IconRight");
            txtUsernameForm.IconRightSize = new Size(30, 30);
            txtUsernameForm.Location = new Point(20, 74);
            txtUsernameForm.Margin = new Padding(4);
            txtUsernameForm.Name = "txtUsernameForm";
            txtUsernameForm.PlaceholderText = "Username";
            txtUsernameForm.SelectedText = "";
            txtUsernameForm.ShadowDecoration.CustomizableEdges = customizableEdges6;
            txtUsernameForm.Size = new Size(281, 38);
            txtUsernameForm.TabIndex = 2;
            // 
            // txtPasswordForm
            // 
            txtPasswordForm.BorderRadius = 18;
            txtPasswordForm.CustomizableEdges = customizableEdges7;
            txtPasswordForm.DefaultText = "";
            txtPasswordForm.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtPasswordForm.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtPasswordForm.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtPasswordForm.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtPasswordForm.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPasswordForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtPasswordForm.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPasswordForm.IconRight = (Image)resources.GetObject("txtPasswordForm.IconRight");
            txtPasswordForm.IconRightSize = new Size(30, 30);
            txtPasswordForm.Location = new Point(20, 120);
            txtPasswordForm.Margin = new Padding(4);
            txtPasswordForm.Name = "txtPasswordForm";
            txtPasswordForm.PasswordChar = '*';
            txtPasswordForm.PlaceholderText = "Password";
            txtPasswordForm.SelectedText = "";
            txtPasswordForm.ShadowDecoration.CustomizableEdges = customizableEdges8;
            txtPasswordForm.Size = new Size(281, 38);
            txtPasswordForm.TabIndex = 4;
            // 
            // plLoginForm
            // 
            plLoginForm.BackColor = Color.Transparent;
            plLoginForm.BackgroundImage = (Image)resources.GetObject("plLoginForm.BackgroundImage");
            plLoginForm.BackgroundImageLayout = ImageLayout.Stretch;
            plLoginForm.Controls.Add(label1);
            plLoginForm.Controls.Add(txtPasswordForm);
            plLoginForm.Controls.Add(txtUsernameForm);
            plLoginForm.Controls.Add(llblRegister);
            plLoginForm.Controls.Add(llblForgotPassword);
            plLoginForm.Controls.Add(btnLogin);
            plLoginForm.Controls.Add(lblLoginForm);
            plLoginForm.Location = new Point(40, 98);
            plLoginForm.Name = "plLoginForm";
            plLoginForm.Size = new Size(321, 290);
            plLoginForm.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.White;
            label1.Location = new Point(53, 243);
            label1.Name = "label1";
            label1.Size = new Size(143, 20);
            label1.TabIndex = 7;
            label1.Text = "Don't have account?";
            // 
            // plHeader
            // 
            plHeader.BackColor = Color.FromArgb(10, 18, 80);
            plHeader.Controls.Add(label2);
            plHeader.Controls.Add(btnClose);
            plHeader.Dock = DockStyle.Top;
            plHeader.Location = new Point(0, 0);
            plHeader.Name = "plHeader";
            plHeader.Size = new Size(831, 37);
            plHeader.TabIndex = 7;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            label2.ForeColor = Color.White;
            label2.Location = new Point(12, 7);
            label2.Name = "label2";
            label2.Size = new Size(70, 20);
            label2.TabIndex = 1;
            label2.Text = "CrypTalk";
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(10, 18, 80);
            btnClose.BackgroundImage = (Image)resources.GetObject("btnClose.BackgroundImage");
            btnClose.BackgroundImageLayout = ImageLayout.Center;
            btnClose.FlatStyle = FlatStyle.Popup;
            btnClose.Location = new Point(787, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(32, 31);
            btnClose.TabIndex = 0;
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
            // 
            // Login
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(831, 487);
            ControlBox = false;
            Controls.Add(plHeader);
            Controls.Add(plLoginForm);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Login";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CrypTalk";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            plLoginForm.ResumeLayout(false);
            plLoginForm.PerformLayout();
            plHeader.ResumeLayout(false);
            plHeader.PerformLayout();
            ResumeLayout(false);
        }




        #endregion
        private PictureBox pictureBox1;
        private Label lblLoginForm;
        private Button btnLogin;
        private LinkLabel llblForgotPassword;
        private LinkLabel llblRegister;
        private Guna.UI2.WinForms.Guna2TextBox txtUsernameForm;
        private Guna.UI2.WinForms.Guna2TextBox txtPasswordForm;
        private Panel plLoginForm;
        private Label label1;
        private Panel plHeader;
        private Button btnClose;
        private Label label2;
    }
}
