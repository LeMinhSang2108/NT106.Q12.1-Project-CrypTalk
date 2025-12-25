namespace Cryptalk
{
    partial class ForgotPassword
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ForgotPassword));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges7 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges8 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            pictureBox1 = new PictureBox();
            plLoginForm = new Panel();
            txtOTP = new Guna.UI2.WinForms.Guna2TextBox();
            txtConfirmPassword = new Guna.UI2.WinForms.Guna2TextBox();
            txtNewPassword = new Guna.UI2.WinForms.Guna2TextBox();
            txtUsernameForm = new Guna.UI2.WinForms.Guna2TextBox();
            btnForgotPassword = new Button();
            lblLoginForm = new Label();
            plHeader = new Panel();
            btnClose = new Button();
            label2 = new Label();
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
            pictureBox1.Size = new Size(800, 450);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // plLoginForm
            // 
            plLoginForm.BackColor = Color.Transparent;
            plLoginForm.BackgroundImage = (Image)resources.GetObject("plLoginForm.BackgroundImage");
            plLoginForm.BackgroundImageLayout = ImageLayout.Stretch;
            plLoginForm.Controls.Add(txtOTP);
            plLoginForm.Controls.Add(txtConfirmPassword);
            plLoginForm.Controls.Add(txtNewPassword);
            plLoginForm.Controls.Add(txtUsernameForm);
            plLoginForm.Controls.Add(btnForgotPassword);
            plLoginForm.Controls.Add(lblLoginForm);
            plLoginForm.Location = new Point(30, 82);
            plLoginForm.Name = "plLoginForm";
            plLoginForm.Size = new Size(321, 350);
            plLoginForm.TabIndex = 8;
            // 
            // txtOTP
            // 
            txtOTP.BorderRadius = 18;
            txtOTP.CustomizableEdges = customizableEdges1;
            txtOTP.DefaultText = "";
            txtOTP.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtOTP.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtOTP.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtOTP.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtOTP.Enabled = false;
            txtOTP.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtOTP.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtOTP.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtOTP.IconRight = (Image)resources.GetObject("txtOTP.IconRight");
            txtOTP.IconRightSize = new Size(30, 30);
            txtOTP.Location = new Point(19, 126);
            txtOTP.Margin = new Padding(4);
            txtOTP.MaxLength = 6;
            txtOTP.Name = "txtOTP";
            txtOTP.PlaceholderText = "Enter OTP Code";
            txtOTP.SelectedText = "";
            txtOTP.ShadowDecoration.CustomizableEdges = customizableEdges2;
            txtOTP.Size = new Size(281, 38);
            txtOTP.TabIndex = 7;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.BorderRadius = 18;
            txtConfirmPassword.CustomizableEdges = customizableEdges3;
            txtConfirmPassword.DefaultText = "";
            txtConfirmPassword.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtConfirmPassword.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtConfirmPassword.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtConfirmPassword.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtConfirmPassword.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtConfirmPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            txtConfirmPassword.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtConfirmPassword.IconRight = (Image)resources.GetObject("txtConfirmPassword.IconRight");
            txtConfirmPassword.IconRightSize = new Size(30, 30);
            txtConfirmPassword.Location = new Point(21, 223);
            txtConfirmPassword.Margin = new Padding(3, 4, 3, 4);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PasswordChar = '*';
            txtConfirmPassword.PlaceholderText = "Confirm password";
            txtConfirmPassword.SelectedText = "";
            txtConfirmPassword.ShadowDecoration.CustomizableEdges = customizableEdges4;
            txtConfirmPassword.Size = new Size(279, 38);
            txtConfirmPassword.TabIndex = 6;
            // 
            // txtNewPassword
            // 
            txtNewPassword.BorderRadius = 18;
            txtNewPassword.CustomizableEdges = customizableEdges5;
            txtNewPassword.DefaultText = "";
            txtNewPassword.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtNewPassword.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtNewPassword.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtNewPassword.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtNewPassword.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtNewPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtNewPassword.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtNewPassword.IconRight = (Image)resources.GetObject("txtNewPassword.IconRight");
            txtNewPassword.IconRightSize = new Size(30, 30);
            txtNewPassword.Location = new Point(19, 172);
            txtNewPassword.Margin = new Padding(4);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.PasswordChar = '*';
            txtNewPassword.PlaceholderText = "New password";
            txtNewPassword.SelectedText = "";
            txtNewPassword.ShadowDecoration.CustomizableEdges = customizableEdges6;
            txtNewPassword.Size = new Size(281, 38);
            txtNewPassword.TabIndex = 4;
            // 
            // txtUsernameForm
            // 
            txtUsernameForm.BorderRadius = 18;
            txtUsernameForm.CustomizableEdges = customizableEdges7;
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
            txtUsernameForm.Location = new Point(19, 80);
            txtUsernameForm.Margin = new Padding(4);
            txtUsernameForm.Name = "txtUsernameForm";
            txtUsernameForm.PlaceholderText = "Username";
            txtUsernameForm.SelectedText = "";
            txtUsernameForm.ShadowDecoration.CustomizableEdges = customizableEdges8;
            txtUsernameForm.Size = new Size(281, 38);
            txtUsernameForm.TabIndex = 2;
            // 
            // btnForgotPassword
            // 
            btnForgotPassword.FlatStyle = FlatStyle.Flat;
            btnForgotPassword.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnForgotPassword.ForeColor = Color.White;
            btnForgotPassword.Location = new Point(21, 283);
            btnForgotPassword.Name = "btnForgotPassword";
            btnForgotPassword.Size = new Size(281, 31);
            btnForgotPassword.TabIndex = 5;
            btnForgotPassword.Text = "FORGOT PASSWORD";
            btnForgotPassword.UseVisualStyleBackColor = true;
            btnForgotPassword.Click += btnForgotPassword_Click;
            btnForgotPassword.MouseEnter += btnForgotPassword_MouseEnter;
            btnForgotPassword.MouseLeave += btnForgotPassword_MouseLeave;
            // 
            // lblLoginForm
            // 
            lblLoginForm.AutoSize = true;
            lblLoginForm.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblLoginForm.ForeColor = Color.White;
            lblLoginForm.Location = new Point(64, 34);
            lblLoginForm.Name = "lblLoginForm";
            lblLoginForm.Size = new Size(193, 31);
            lblLoginForm.TabIndex = 1;
            lblLoginForm.Text = "Forgot password";
            // 
            // plHeader
            // 
            plHeader.BackColor = Color.FromArgb(10, 18, 80);
            plHeader.Controls.Add(btnClose);
            plHeader.Controls.Add(label2);
            plHeader.Dock = DockStyle.Top;
            plHeader.Location = new Point(0, 0);
            plHeader.Name = "plHeader";
            plHeader.Size = new Size(800, 37);
            plHeader.TabIndex = 9;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(10, 18, 80);
            btnClose.BackgroundImage = (Image)resources.GetObject("btnClose.BackgroundImage");
            btnClose.BackgroundImageLayout = ImageLayout.Center;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Location = new Point(762, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(26, 29);
            btnClose.TabIndex = 2;
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += btnClose_Click;
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
            // ForgotPassword
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            ControlBox = false;
            Controls.Add(plHeader);
            Controls.Add(plLoginForm);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "ForgotPassword";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "s";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            plLoginForm.ResumeLayout(false);
            plLoginForm.PerformLayout();
            plHeader.ResumeLayout(false);
            plHeader.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox pictureBox1;
        private Panel plLoginForm;
        private Guna.UI2.WinForms.Guna2TextBox txtOTP;
        private Guna.UI2.WinForms.Guna2TextBox txtConfirmPassword;
        private Guna.UI2.WinForms.Guna2TextBox txtNewPassword;
        private Guna.UI2.WinForms.Guna2TextBox txtUsernameForm;
        private Button btnForgotPassword;
        private Label lblLoginForm;
        private Panel plHeader;
        private Label label2;
        private Button btnClose;
    }
}