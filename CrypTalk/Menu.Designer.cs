namespace Cryptalk
{
    partial class Menu
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Menu));
            panel1 = new Panel();
            panel2 = new Panel();
            nightControlBox1 = new ReaLTaiizor.Controls.NightControlBox();
            lblTitleSidebar = new Label();
            btnSide = new PictureBox();
            sidebar = new FlowLayoutPanel();
            pnDashboard = new Panel();
            btnDashboard = new Button();
            menuContainer = new FlowLayoutPanel();
            panel3 = new Panel();
            btnMenu = new Button();
            panel7 = new Panel();
            btnLogin = new Button();
            panel8 = new Panel();
            btnRegister = new Button();
            panel9 = new Panel();
            btnForgotPassword = new Button();
            pnChat = new Panel();
            btnChat = new Button();
            pnProfile = new Panel();
            btnAudioCall = new Button();
            panel5 = new Panel();
            btnNetworkDiag = new Button();
            pnLogout = new Panel();
            btnProfile = new Button();
            panel4 = new Panel();
            btnLogout = new Button();
            menuTransition = new System.Windows.Forms.Timer(components);
            sidebarTransition = new System.Windows.Forms.Timer(components);
            txtCopyright = new TextBox();
            footerPanel = new Panel();
            lblCopyright = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)btnSide).BeginInit();
            sidebar.SuspendLayout();
            pnDashboard.SuspendLayout();
            menuContainer.SuspendLayout();
            panel3.SuspendLayout();
            panel7.SuspendLayout();
            panel8.SuspendLayout();
            panel9.SuspendLayout();
            pnChat.SuspendLayout();
            pnProfile.SuspendLayout();
            panel5.SuspendLayout();
            pnLogout.SuspendLayout();
            panel4.SuspendLayout();
            footerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(10, 18, 80);
            panel1.Controls.Add(panel2);
            panel1.Controls.Add(nightControlBox1);
            panel1.Controls.Add(lblTitleSidebar);
            panel1.Controls.Add(btnSide);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1077, 43);
            panel1.TabIndex = 0;
            // 
            // panel2
            // 
            panel2.Location = new Point(257, 43);
            panel2.Name = "panel2";
            panel2.Size = new Size(850, 547);
            panel2.TabIndex = 2;
            // 
            // nightControlBox1
            // 
            nightControlBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nightControlBox1.BackColor = Color.Transparent;
            nightControlBox1.CloseHoverColor = Color.FromArgb(199, 80, 80);
            nightControlBox1.CloseHoverForeColor = Color.White;
            nightControlBox1.DefaultLocation = true;
            nightControlBox1.DisableMaximizeColor = Color.FromArgb(105, 105, 105);
            nightControlBox1.DisableMinimizeColor = Color.FromArgb(105, 105, 105);
            nightControlBox1.EnableCloseColor = Color.FromArgb(160, 160, 160);
            nightControlBox1.EnableMaximizeButton = true;
            nightControlBox1.EnableMaximizeColor = Color.FromArgb(160, 160, 160);
            nightControlBox1.EnableMinimizeButton = true;
            nightControlBox1.EnableMinimizeColor = Color.FromArgb(160, 160, 160);
            nightControlBox1.Location = new Point(938, 0);
            nightControlBox1.MaximizeHoverColor = Color.FromArgb(15, 255, 255, 255);
            nightControlBox1.MaximizeHoverForeColor = Color.White;
            nightControlBox1.MinimizeHoverColor = Color.FromArgb(15, 255, 255, 255);
            nightControlBox1.MinimizeHoverForeColor = Color.White;
            nightControlBox1.Name = "nightControlBox1";
            nightControlBox1.Size = new Size(139, 31);
            nightControlBox1.TabIndex = 3;
            // 
            // lblTitleSidebar
            // 
            lblTitleSidebar.AutoSize = true;
            lblTitleSidebar.Font = new Font("Segoe UI", 10.2F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lblTitleSidebar.ForeColor = Color.White;
            lblTitleSidebar.Location = new Point(47, 10);
            lblTitleSidebar.Name = "lblTitleSidebar";
            lblTitleSidebar.Size = new Size(158, 23);
            lblTitleSidebar.TabIndex = 2;
            lblTitleSidebar.Text = "Cryptalk | Sidebar";
            // 
            // btnSide
            // 
            btnSide.Image = (Image)resources.GetObject("btnSide.Image");
            btnSide.Location = new Point(4, 7);
            btnSide.Name = "btnSide";
            btnSide.Size = new Size(37, 29);
            btnSide.SizeMode = PictureBoxSizeMode.CenterImage;
            btnSide.TabIndex = 1;
            btnSide.TabStop = false;
            btnSide.Click += btnSide_Click;
            // 
            // sidebar
            // 
            sidebar.BackColor = Color.FromArgb(10, 18, 80);
            sidebar.Controls.Add(pnDashboard);
            sidebar.Controls.Add(menuContainer);
            sidebar.Controls.Add(pnChat);
            sidebar.Controls.Add(pnProfile);
            sidebar.Controls.Add(panel5);
            sidebar.Controls.Add(pnLogout);
            sidebar.Controls.Add(panel4);
            sidebar.Dock = DockStyle.Left;
            sidebar.Location = new Point(0, 43);
            sidebar.Name = "sidebar";
            sidebar.Size = new Size(254, 451);
            sidebar.TabIndex = 1;
            // 
            // pnDashboard
            // 
            pnDashboard.BackColor = Color.FromArgb(10, 18, 80);
            pnDashboard.Controls.Add(btnDashboard);
            pnDashboard.Location = new Point(3, 3);
            pnDashboard.Name = "pnDashboard";
            pnDashboard.Size = new Size(254, 55);
            pnDashboard.TabIndex = 3;
            // 
            // btnDashboard
            // 
            btnDashboard.BackColor = Color.FromArgb(10, 18, 80);
            btnDashboard.FlatStyle = FlatStyle.Popup;
            btnDashboard.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnDashboard.ForeColor = Color.Transparent;
            btnDashboard.Image = (Image)resources.GetObject("btnDashboard.Image");
            btnDashboard.ImageAlign = ContentAlignment.MiddleLeft;
            btnDashboard.Location = new Point(0, 0);
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(254, 55);
            btnDashboard.TabIndex = 2;
            btnDashboard.Text = "        Dashboard";
            btnDashboard.TextAlign = ContentAlignment.MiddleLeft;
            btnDashboard.UseVisualStyleBackColor = false;
            btnDashboard.Click += btnDashboard_Click;
            // 
            // menuContainer
            // 
            menuContainer.BackColor = Color.FromArgb(10, 18, 80);
            menuContainer.Controls.Add(panel3);
            menuContainer.Controls.Add(panel7);
            menuContainer.Controls.Add(panel8);
            menuContainer.Controls.Add(panel9);
            menuContainer.Location = new Point(3, 64);
            menuContainer.Name = "menuContainer";
            menuContainer.Size = new Size(254, 55);
            menuContainer.TabIndex = 7;
            // 
            // panel3
            // 
            panel3.BackColor = Color.FromArgb(10, 18, 80);
            panel3.Controls.Add(btnMenu);
            panel3.Location = new Point(3, 3);
            panel3.Name = "panel3";
            panel3.Size = new Size(254, 55);
            panel3.TabIndex = 4;
            // 
            // btnMenu
            // 
            btnMenu.BackColor = Color.FromArgb(10, 18, 80);
            btnMenu.FlatStyle = FlatStyle.Popup;
            btnMenu.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnMenu.ForeColor = Color.Transparent;
            btnMenu.Image = (Image)resources.GetObject("btnMenu.Image");
            btnMenu.ImageAlign = ContentAlignment.MiddleLeft;
            btnMenu.Location = new Point(-3, -3);
            btnMenu.Margin = new Padding(0);
            btnMenu.Name = "btnMenu";
            btnMenu.Size = new Size(254, 55);
            btnMenu.TabIndex = 2;
            btnMenu.Text = "        Menu";
            btnMenu.TextAlign = ContentAlignment.MiddleLeft;
            btnMenu.UseVisualStyleBackColor = false;
            btnMenu.Click += btnMenu_Click;
            // 
            // panel7
            // 
            panel7.BackColor = Color.FromArgb(10, 18, 80);
            panel7.Controls.Add(btnLogin);
            panel7.Location = new Point(3, 64);
            panel7.Name = "panel7";
            panel7.Size = new Size(254, 55);
            panel7.TabIndex = 5;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(10, 18, 80);
            btnLogin.FlatStyle = FlatStyle.Popup;
            btnLogin.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnLogin.ForeColor = Color.Transparent;
            btnLogin.Image = (Image)resources.GetObject("btnLogin.Image");
            btnLogin.ImageAlign = ContentAlignment.MiddleLeft;
            btnLogin.Location = new Point(0, 0);
            btnLogin.Margin = new Padding(0);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(254, 55);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "        Login";
            btnLogin.TextAlign = ContentAlignment.MiddleLeft;
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // panel8
            // 
            panel8.BackColor = Color.FromArgb(10, 18, 80);
            panel8.Controls.Add(btnRegister);
            panel8.Location = new Point(3, 125);
            panel8.Name = "panel8";
            panel8.Size = new Size(254, 55);
            panel8.TabIndex = 6;
            // 
            // btnRegister
            // 
            btnRegister.BackColor = Color.FromArgb(10, 18, 80);
            btnRegister.FlatStyle = FlatStyle.Popup;
            btnRegister.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnRegister.ForeColor = Color.Transparent;
            btnRegister.Image = (Image)resources.GetObject("btnRegister.Image");
            btnRegister.ImageAlign = ContentAlignment.MiddleLeft;
            btnRegister.Location = new Point(0, 0);
            btnRegister.Margin = new Padding(0);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new Size(254, 55);
            btnRegister.TabIndex = 2;
            btnRegister.Text = "        Register";
            btnRegister.TextAlign = ContentAlignment.MiddleLeft;
            btnRegister.UseVisualStyleBackColor = false;
            btnRegister.Click += btnRegister_Click;
            // 
            // panel9
            // 
            panel9.BackColor = Color.FromArgb(10, 18, 80);
            panel9.Controls.Add(btnForgotPassword);
            panel9.Location = new Point(3, 186);
            panel9.Name = "panel9";
            panel9.Size = new Size(254, 55);
            panel9.TabIndex = 7;
            // 
            // btnForgotPassword
            // 
            btnForgotPassword.BackColor = Color.FromArgb(10, 18, 80);
            btnForgotPassword.FlatStyle = FlatStyle.Popup;
            btnForgotPassword.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnForgotPassword.ForeColor = Color.Transparent;
            btnForgotPassword.Image = (Image)resources.GetObject("btnForgotPassword.Image");
            btnForgotPassword.ImageAlign = ContentAlignment.MiddleLeft;
            btnForgotPassword.Location = new Point(0, 0);
            btnForgotPassword.Margin = new Padding(0);
            btnForgotPassword.Name = "btnForgotPassword";
            btnForgotPassword.Size = new Size(254, 55);
            btnForgotPassword.TabIndex = 2;
            btnForgotPassword.Text = "        Forgot Password";
            btnForgotPassword.TextAlign = ContentAlignment.MiddleLeft;
            btnForgotPassword.UseVisualStyleBackColor = false;
            btnForgotPassword.Click += btnForgotPassword_Click;
            // 
            // pnChat
            // 
            pnChat.BackColor = Color.FromArgb(10, 18, 80);
            pnChat.Controls.Add(btnChat);
            pnChat.Location = new Point(3, 125);
            pnChat.Name = "pnChat";
            pnChat.Size = new Size(254, 55);
            pnChat.TabIndex = 6;
            // 
            // btnChat
            // 
            btnChat.BackColor = Color.FromArgb(10, 18, 80);
            btnChat.FlatStyle = FlatStyle.Popup;
            btnChat.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnChat.ForeColor = Color.Transparent;
            btnChat.Image = (Image)resources.GetObject("btnChat.Image");
            btnChat.ImageAlign = ContentAlignment.MiddleLeft;
            btnChat.Location = new Point(0, 0);
            btnChat.Name = "btnChat";
            btnChat.Size = new Size(254, 55);
            btnChat.TabIndex = 2;
            btnChat.Text = "        Chat";
            btnChat.TextAlign = ContentAlignment.MiddleLeft;
            btnChat.UseVisualStyleBackColor = false;
            btnChat.Click += btnChat_Click;
            // 
            // pnProfile
            // 
            pnProfile.BackColor = Color.FromArgb(10, 18, 80);
            pnProfile.Controls.Add(btnAudioCall);
            pnProfile.Location = new Point(3, 186);
            pnProfile.Name = "pnProfile";
            pnProfile.Size = new Size(254, 55);
            pnProfile.TabIndex = 4;
            // 
            // btnAudioCall
            // 
            btnAudioCall.BackColor = Color.FromArgb(10, 18, 80);
            btnAudioCall.FlatStyle = FlatStyle.Popup;
            btnAudioCall.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnAudioCall.ForeColor = Color.Transparent;
            btnAudioCall.Image = (Image)resources.GetObject("btnAudioCall.Image");
            btnAudioCall.ImageAlign = ContentAlignment.MiddleLeft;
            btnAudioCall.Location = new Point(0, 0);
            btnAudioCall.Name = "btnAudioCall";
            btnAudioCall.Size = new Size(254, 55);
            btnAudioCall.TabIndex = 3;
            btnAudioCall.Text = "        Audio Call";
            btnAudioCall.TextAlign = ContentAlignment.MiddleLeft;
            btnAudioCall.UseVisualStyleBackColor = false;
            btnAudioCall.Click += btnAudioCall_Click;
            // 
            // panel5
            // 
            panel5.BackColor = Color.FromArgb(10, 18, 80);
            panel5.Controls.Add(btnNetworkDiag);
            panel5.Location = new Point(3, 247);
            panel5.Name = "panel5";
            panel5.Size = new Size(254, 55);
            panel5.TabIndex = 7;
            // 
            // btnNetworkDiag
            // 
            btnNetworkDiag.BackColor = Color.FromArgb(10, 18, 80);
            btnNetworkDiag.FlatStyle = FlatStyle.Popup;
            btnNetworkDiag.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnNetworkDiag.ForeColor = Color.Transparent;
            btnNetworkDiag.Image = (Image)resources.GetObject("btnNetworkDiag.Image");
            btnNetworkDiag.ImageAlign = ContentAlignment.MiddleLeft;
            btnNetworkDiag.Location = new Point(0, 0);
            btnNetworkDiag.Name = "btnNetworkDiag";
            btnNetworkDiag.Size = new Size(254, 55);
            btnNetworkDiag.TabIndex = 2;
            btnNetworkDiag.Text = "        Network Diagnostic";
            btnNetworkDiag.TextAlign = ContentAlignment.MiddleLeft;
            btnNetworkDiag.UseVisualStyleBackColor = false;
            btnNetworkDiag.Click += btnNetworkDiag_Click;
            // 
            // pnLogout
            // 
            pnLogout.BackColor = Color.FromArgb(10, 18, 80);
            pnLogout.Controls.Add(btnProfile);
            pnLogout.Location = new Point(3, 308);
            pnLogout.Name = "pnLogout";
            pnLogout.Size = new Size(254, 55);
            pnLogout.TabIndex = 5;
            // 
            // btnProfile
            // 
            btnProfile.BackColor = Color.FromArgb(10, 18, 80);
            btnProfile.FlatStyle = FlatStyle.Popup;
            btnProfile.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnProfile.ForeColor = Color.Transparent;
            btnProfile.Image = (Image)resources.GetObject("btnProfile.Image");
            btnProfile.ImageAlign = ContentAlignment.MiddleLeft;
            btnProfile.Location = new Point(1, 0);
            btnProfile.Name = "btnProfile";
            btnProfile.Size = new Size(254, 55);
            btnProfile.TabIndex = 2;
            btnProfile.Text = "        Profile";
            btnProfile.TextAlign = ContentAlignment.MiddleLeft;
            btnProfile.UseVisualStyleBackColor = false;
            btnProfile.Click += btnProfile_Click;
            // 
            // panel4
            // 
            panel4.BackColor = Color.FromArgb(10, 18, 80);
            panel4.Controls.Add(btnLogout);
            panel4.Location = new Point(3, 369);
            panel4.Name = "panel4";
            panel4.Size = new Size(254, 55);
            panel4.TabIndex = 6;
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.FromArgb(10, 18, 80);
            btnLogout.FlatStyle = FlatStyle.Popup;
            btnLogout.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 163);
            btnLogout.ForeColor = Color.Transparent;
            btnLogout.Image = (Image)resources.GetObject("btnLogout.Image");
            btnLogout.ImageAlign = ContentAlignment.MiddleLeft;
            btnLogout.Location = new Point(0, 0);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(254, 55);
            btnLogout.TabIndex = 2;
            btnLogout.Text = "        Logout";
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // menuTransition
            // 
            menuTransition.Interval = 10;
            menuTransition.Tick += menuTransition_Tick;
            // 
            // sidebarTransition
            // 
            sidebarTransition.Interval = 10;
            sidebarTransition.Tick += sidebarTransition_Tick;
            // 
            // txtCopyright
            // 
            txtCopyright.Location = new Point(549, 428);
            txtCopyright.Name = "txtCopyright";
            txtCopyright.Size = new Size(272, 27);
            txtCopyright.TabIndex = 2;
            txtCopyright.Text = "© 2025 made with ❤️by f4ng_snyder";
            txtCopyright.TextAlign = HorizontalAlignment.Center;
            // 
            // footerPanel
            // 
            footerPanel.BackColor = Color.FromArgb(15, 25, 95);
            footerPanel.Controls.Add(lblCopyright);
            footerPanel.Dock = DockStyle.Bottom;
            footerPanel.Location = new Point(0, 494);
            footerPanel.Name = "footerPanel";
            footerPanel.Padding = new Padding(10);
            footerPanel.Size = new Size(1077, 50);
            footerPanel.TabIndex = 2;
            // 
            // lblCopyright
            // 
            lblCopyright.Dock = DockStyle.Fill;
            lblCopyright.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCopyright.ForeColor = Color.FromArgb(180, 180, 180);
            lblCopyright.Location = new Point(10, 10);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(1057, 30);
            lblCopyright.TabIndex = 0;
            lblCopyright.Text = "© 2025 made with ❤️ by f4ng_snyder";
            lblCopyright.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Menu
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1077, 544);
            Controls.Add(sidebar);
            Controls.Add(panel1);
            Controls.Add(footerPanel);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Name = "Menu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Menu";
            Load += Menu_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)btnSide).EndInit();
            sidebar.ResumeLayout(false);
            pnDashboard.ResumeLayout(false);
            menuContainer.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel7.ResumeLayout(false);
            panel8.ResumeLayout(false);
            panel9.ResumeLayout(false);
            pnChat.ResumeLayout(false);
            pnProfile.ResumeLayout(false);
            panel5.ResumeLayout(false);
            pnLogout.ResumeLayout(false);
            panel4.ResumeLayout(false);
            footerPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private PictureBox btnSide;
        private Label lblTitleSidebar;
        private ReaLTaiizor.Controls.NightControlBox nightControlBox1;
        private FlowLayoutPanel sidebar;
        private Button btnDashboard;
        private Panel pnDashboard;
        private Panel panel3;
        private Panel pnProfile;
        private Panel pnLogout;
        private Button btnMenu;
        private Button btnProfile;
        private Button btnLogout;
        private System.Windows.Forms.Timer menuTransition;
        private FlowLayoutPanel menuContainer;
        private Panel panel7;
        private Button btnLogin;
        private Panel panel8;
        private Button btnRegister;
        private Panel panel9;
        private Button btnForgotPassword;
        private System.Windows.Forms.Timer sidebarTransition;
        private Panel pnChat;
        private Button btnChat;
        private Panel panel2;
        private Button btnAudioCall;
        private Panel panel4;
        private Panel panel5;
        private Button btnNetworkDiag;
        private TextBox txtCopyright;
        private Panel footerPanel;
        private Label lblCopyright;
    }
}