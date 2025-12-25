namespace Cryptalk
{
    partial class Register
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Register));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            pictureBox1 = new PictureBox();
            plLoginForm = new Panel();
            txtConfirmPassword = new Guna.UI2.WinForms.Guna2TextBox();
            txtPasswordForm = new Guna.UI2.WinForms.Guna2TextBox();
            txtUsernameForm = new Guna.UI2.WinForms.Guna2TextBox();
            btnRegister = new Button();
            lblLoginForm = new Label();
            plHeader = new Panel();
            btnClose = new Button();
            label2 = new Label();
            button1 = new Button();
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
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // plLoginForm
            // 
            plLoginForm.BackColor = Color.Transparent;
            plLoginForm.BackgroundImage = (Image)resources.GetObject("plLoginForm.BackgroundImage");
            plLoginForm.BackgroundImageLayout = ImageLayout.Stretch;
            plLoginForm.Controls.Add(txtConfirmPassword);
            plLoginForm.Controls.Add(txtPasswordForm);
            plLoginForm.Controls.Add(txtUsernameForm);
            plLoginForm.Controls.Add(btnRegister);
            plLoginForm.Controls.Add(lblLoginForm);
            plLoginForm.Location = new Point(28, 74);
            plLoginForm.Name = "plLoginForm";
            plLoginForm.Size = new Size(321, 302);
            plLoginForm.TabIndex = 7;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.BorderRadius = 18;
            txtConfirmPassword.CustomizableEdges = customizableEdges1;
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
            txtConfirmPassword.Location = new Point(21, 172);
            txtConfirmPassword.Margin = new Padding(3, 4, 3, 4);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PasswordChar = '*';
            txtConfirmPassword.PlaceholderText = "Confirm password";
            txtConfirmPassword.SelectedText = "";
            txtConfirmPassword.ShadowDecoration.CustomizableEdges = customizableEdges2;
            txtConfirmPassword.Size = new Size(279, 38);
            txtConfirmPassword.TabIndex = 6;
            // 
            // txtPasswordForm
            // 
            txtPasswordForm.BorderRadius = 18;
            txtPasswordForm.CustomizableEdges = customizableEdges3;
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
            txtPasswordForm.Location = new Point(21, 126);
            txtPasswordForm.Margin = new Padding(4);
            txtPasswordForm.Name = "txtPasswordForm";
            txtPasswordForm.PasswordChar = '*';
            txtPasswordForm.PlaceholderText = "Password";
            txtPasswordForm.SelectedText = "";
            txtPasswordForm.ShadowDecoration.CustomizableEdges = customizableEdges4;
            txtPasswordForm.Size = new Size(281, 38);
            txtPasswordForm.TabIndex = 4;
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
            txtUsernameForm.Location = new Point(19, 80);
            txtUsernameForm.Margin = new Padding(4);
            txtUsernameForm.Name = "txtUsernameForm";
            txtUsernameForm.PlaceholderText = "Username";
            txtUsernameForm.SelectedText = "";
            txtUsernameForm.ShadowDecoration.CustomizableEdges = customizableEdges6;
            txtUsernameForm.Size = new Size(281, 38);
            txtUsernameForm.TabIndex = 2;
            // 
            // btnRegister
            // 
            btnRegister.FlatStyle = FlatStyle.Flat;
            btnRegister.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnRegister.ForeColor = Color.White;
            btnRegister.Location = new Point(21, 237);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(281, 31);
            btnRegister.TabIndex = 5;
            btnRegister.Text = "REGISTER";
            btnRegister.UseVisualStyleBackColor = true;
            btnRegister.Click += btnRegister_Click;
            btnRegister.MouseEnter += btnRegister_MouseEnter;
            btnRegister.MouseLeave += btnRegister_MouseLeave;
            // 
            // lblLoginForm
            // 
            lblLoginForm.AutoSize = true;
            lblLoginForm.Font = new Font("Segoe UI", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblLoginForm.ForeColor = Color.White;
            lblLoginForm.Location = new Point(120, 34);
            lblLoginForm.Name = "lblLoginForm";
            lblLoginForm.Size = new Size(101, 31);
            lblLoginForm.TabIndex = 1;
            lblLoginForm.Text = "Register";
            // 
            // plHeader
            // 
            plHeader.BackColor = Color.FromArgb(10, 18, 80);
            plHeader.Controls.Add(btnClose);
            plHeader.Controls.Add(label2);
            plHeader.Controls.Add(button1);
            plHeader.Dock = DockStyle.Top;
            plHeader.Location = new Point(0, 0);
            plHeader.Name = "plHeader";
            plHeader.Size = new Size(800, 37);
            plHeader.TabIndex = 8;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(10, 18, 80);
            btnClose.BackgroundImage = (Image)resources.GetObject("btnClose.BackgroundImage");
            btnClose.BackgroundImageLayout = ImageLayout.Center;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Location = new Point(763, 3);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(25, 29);
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
            // button1
            // 
            button1.BackColor = Color.FromArgb(10, 18, 80);
            button1.BackgroundImage = (Image)resources.GetObject("button1.BackgroundImage");
            button1.BackgroundImageLayout = ImageLayout.Center;
            button1.FlatStyle = FlatStyle.Flat;
            button1.Location = new Point(782, 3);
            button1.Name = "button1";
            button1.Size = new Size(59, 29);
            button1.TabIndex = 0;
            button1.UseVisualStyleBackColor = false;
            // 
            // Register
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            ControlBox = false;
            Controls.Add(plHeader);
            Controls.Add(plLoginForm);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Register";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Register";
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
        private Guna.UI2.WinForms.Guna2TextBox txtPasswordForm;
        private Guna.UI2.WinForms.Guna2TextBox txtUsernameForm;
        private Button btnRegister;
        private Label lblLoginForm;
        private Guna.UI2.WinForms.Guna2TextBox txtConfirmPassword;
        private Panel plHeader;
        private Label label2;
        private Button button1;
        private Button btnClose;
    }
}