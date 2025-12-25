using Audio___Video_Calling_app;
using ChatApp;
using CrypTalk;
using Firebase.Database.Query;
using PingTool;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cryptalk
{
    public partial class Menu : Form
    {
        // Child forms
        private ForgotPassword forgotPasswordForm;
        private Login loginForm;
        private Register registerForm;
        private Profile profileForm;
        private VoipTestForm voipTestForm;

        // Current login user info
        public string CurrentUsername { get; set; }
        public bool IsAdmin { get; set; }

        private System.Windows.Forms.Timer sessionCheckTimer;

        // Sidebar user info card
        private Panel userInfoCard;
        private PictureBox pbUserAvatar;
        private Label lblUserName;
        private Label lblUserRole;
        private Button btnAdminPanelSidebar;

        // Main content panel
        private Panel mainContentPanel;
        private Panel userDetailPanel;
        private PictureBox pbLargeAvatar;
        private Label lblDisplayName;
        private Label lblUserEmail;
        private Label lblUserPhone;
        private Label lblUserGender;
        private Label lblMemberSince;
        private Button btnEditProfile;
        private Button btnViewDashboard;

        private static Form activeChatWindow = null;
        public Menu()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
        }

        bool menuExpand = false;

        private void menuTransition_Tick(object sender, EventArgs e)
        {
            if (!menuExpand)
            {
                menuContainer.Height += 10;
                if (menuContainer.Height >= 172)
                {
                    menuTransition.Stop();
                    menuExpand = true;
                }
            }
            else
            {
                menuContainer.Height -= 10;
                if (menuContainer.Height <= 42)
                {
                    menuTransition.Stop();
                    menuExpand = false;
                }
            }
        }

        private void btnMenu_Click(object sender, EventArgs e)
        {
            menuTransition.Start();
        }

        bool sidebarExpand = true;

        private void sidebarTransition_Tick(object sender, EventArgs e)
        {
            if (sidebarExpand)
            {
                sidebar.Width -= 10;
                if (sidebar.Width <= 49)
                {
                    sidebarExpand = false;
                    sidebarTransition.Stop();
                }
            }
            else
            {
                sidebar.Width += 10;
                if (sidebar.Width >= 232)
                {
                    sidebarExpand = true;
                    sidebarTransition.Stop();

                    pnDashboard.Width = sidebar.Width;
                    menuContainer.Width = sidebar.Width;
                    pnChat.Width = sidebar.Width;
                    pnProfile.Width = sidebar.Width;
                    pnLogout.Width = sidebar.Width;

                    if (userInfoCard != null)
                        userInfoCard.Width = sidebar.Width;
                    if (btnAdminPanelSidebar != null)
                        btnAdminPanelSidebar.Width = sidebar.Width;
                }
            }
        }

        private void btnSide_Click(object sender, EventArgs e)
        {
            sidebarTransition.Start();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (loginForm == null || loginForm.IsDisposed)
            {
                loginForm = new Login();
            }
            loginForm.Show();
            this.Hide();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (registerForm == null || registerForm.IsDisposed)
            {
                registerForm = new Register();
            }
            registerForm.Show();
            this.Hide();
        }

        private void btnForgotPassword_Click(object sender, EventArgs e)
        {
            if (forgotPasswordForm == null || forgotPasswordForm.IsDisposed)
            {
                forgotPasswordForm = new ForgotPassword();
            }
            forgotPasswordForm.Show();
            this.Hide();
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentUsername))
            {
                MessageBox.Show("Please login first to view Profile.",
                    "Not Logged In",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.Hide();
            using (Profile profileForm = new Profile(CurrentUsername))
            {
                profileForm.ShowDialog();
            }
            this.Show();

            // Refresh main panel
            if (mainContentPanel != null)
            {
                LoadUserProfileToMainPanel();
            }
        }

        private async void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to log out?",
                "Log out confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
               
                try
                {
                    await FirebaseHelper.firebaseClient
                        .Child("users")
                        .Child(CurrentUsername)
                        .Child("ActiveSession")
                        .DeleteAsync();
                }
                catch { }

                
                FirebaseHelper.ClearSession(CurrentUsername);
                sessionCheckTimer?.Stop();
                CurrentUsername = string.Empty;
                this.Close();

            }
        }

        private void btnChat_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentUsername))
            {
                MessageBox.Show("Please login first to use Chat.");
                return;
            }


            if (activeChatWindow != null && !activeChatWindow.IsDisposed)
            {

                if (activeChatWindow.WindowState == FormWindowState.Minimized)
                    activeChatWindow.WindowState = FormWindowState.Normal;

                activeChatWindow.BringToFront();
                activeChatWindow.Focus();
                return;
            }


            DialogResult choice = MessageBox.Show(
                "Do you want to start as Server?\n\nYes = Server\nNo = Client",
                "E2E Chat",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (choice == DialogResult.Yes)
            {
                ServerForm serverForm = new ServerForm();
                serverForm.Show();

            }
            else
            {

                var clientForm = new ClientForm(CurrentUsername);
                clientForm.Show();
                activeChatWindow = clientForm;
            }
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentUsername))
            {
                MessageBox.Show("Please login first to view Dashboard.",
                    "Not Logged In",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.Hide();
            try
            {
                using (Dashboard dashboardForm = new Dashboard(CurrentUsername, DateTime.Now))
                {
                    dashboardForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Dashboard:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Console.WriteLine($"Dashboard Error: {ex}");
            }
            finally
            {
                this.Show();
            }
        }

        private void btnAudioCall_Click(object sender, EventArgs e)
        {
            VoipTestForm newVoipForm = new VoipTestForm();
            newVoipForm.Show();
        }

        private void btnNetworkDiag_Click(object sender, EventArgs e)
        {
            NetworkDiagnostic networkDiagForm = new NetworkDiagnostic();
            networkDiagForm.Show();
        }

        private void btnAdminPanel_Click(object sender, EventArgs e)
        {
            if (!IsAdmin)
            {
                MessageBox.Show("You do not have admin privileges!",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            this.Hide();
            using (AdminPanel adminForm = new AdminPanel())
            {
                adminForm.ShowDialog();
            }
            this.Show();
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            BuildUserInfoCard();
            BuildAdminPanelButton();
            BuildMainContentPanel();

            sessionCheckTimer = new System.Windows.Forms.Timer();
            sessionCheckTimer.Interval = 1000;
            sessionCheckTimer.Tick += SessionCheckTimer_Tick;
            sessionCheckTimer.Start();
        }

        private async void SessionCheckTimer_Tick(object sender, EventArgs e)
        {
            await CheckSessionValidity();
        }

        private async Task CheckSessionValidity()
        {
            if (string.IsNullOrEmpty(CurrentUsername))
                return;

            bool isValid = await FirebaseHelper.ValidateSession(CurrentUsername);

            if (!isValid)
            {
                sessionCheckTimer?.Stop();

                System.Diagnostics.Debug.WriteLine($"[KICK OUT] User {CurrentUsername} will be logged out!");

                MessageBox.Show(
                    "⚠️ Your account has been logged in from another device!\n\nYou will be logged out.",
                    "Session Expired",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                ForceLogout();
            }
        }

        private void ForceLogout()
        {
            string usernameToLogout = CurrentUsername;
            CurrentUsername = string.Empty;

            if (!string.IsNullOrEmpty(usernameToLogout))
            {
                FirebaseHelper.ClearSession(usernameToLogout);
            }

            Login loginForm = new Login();
            loginForm.Show();
            this.Close();
        }

        private void BuildUserInfoCard()
        {
            userInfoCard = new Panel
            {
                Width = 232,
                Height = 160,
                BackColor = Color.FromArgb(15, 25, 95),
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 0)
            };

            pbUserAvatar = new PictureBox
            {
                Location = new Point(70, 20),
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pbUserAvatar.Width - 1, pbUserAvatar.Height - 1);
            pbUserAvatar.Region = new Region(gp);
            pbUserAvatar.Click += (s, e) => btnProfile_Click(s, e);

            lblUserName = new Label
            {
                Text = string.IsNullOrEmpty(CurrentUsername) ? "Guest" : CurrentUsername,
                Location = new Point(3, 110),
                Size = new Size(212, 22),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };

            lblUserRole = new Label
            {
                Text = IsAdmin ? "👑 ADMIN" : "👤 USER",
                Location = new Point(3, 135),
                Size = new Size(212, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                ForeColor = IsAdmin ? Color.FromArgb(255, 193, 7) : Color.FromArgb(150, 150, 150),
                TextAlign = ContentAlignment.MiddleCenter
            };

            LoadUserAvatar();

            userInfoCard.Controls.Add(pbUserAvatar);
            userInfoCard.Controls.Add(lblUserName);
            userInfoCard.Controls.Add(lblUserRole);
            sidebar.Controls.Add(userInfoCard);
            sidebar.Controls.SetChildIndex(userInfoCard, 0);
        }

        private void BuildAdminPanelButton()
        {
            if (IsAdmin)
            {
                Panel pnlAdminButton = new Panel
                {
                    Width = 232,
                    Height = 50,
                    BackColor = Color.FromArgb(255, 193, 7),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 0, 0, 5)
                };
                pnlAdminButton.Click += btnAdminPanel_Click;

                Label lblIcon = new Label
                {
                    Text = "👑",
                    Font = new Font("Segoe UI Symbol", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(7, 12),
                    Size = new Size(30, 25),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Cursor = Cursors.Hand
                };
                lblIcon.Click += btnAdminPanel_Click;

                Label lblText = new Label
                {
                    Text = "Admin Panel",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(37, 15),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };
                lblText.Click += btnAdminPanel_Click;

                pnlAdminButton.Controls.Add(lblIcon);
                pnlAdminButton.Controls.Add(lblText);

                sidebar.Controls.Add(pnlAdminButton);
                sidebar.Controls.SetChildIndex(pnlAdminButton, 1);
                btnAdminPanelSidebar = new Button();
            }
        }

        private async void LoadUserAvatar()
        {
            if (string.IsNullOrEmpty(CurrentUsername))
            {
                pbUserAvatar.Image = CreateDefaultAvatar(80, 80, "?");
                return;
            }

            try
            {
                var user = await CrypTalk.FirebaseHelper.GetUser(CurrentUsername);

                if (user != null && !string.IsNullOrEmpty(user.Avatar) && System.IO.File.Exists(user.Avatar))
                {
                    pbUserAvatar.Image = Image.FromFile(user.Avatar);
                }
                else
                {
                    pbUserAvatar.Image = CreateDefaultAvatar(80, 80, CurrentUsername);
                }

                lblUserName.Text = CurrentUsername;
                lblUserRole.Text = IsAdmin ? "👑 ADMIN" : "👤 USER";
                lblUserRole.ForeColor = IsAdmin ? Color.FromArgb(255, 193, 7) : Color.FromArgb(150, 150, 150);
            }
            catch
            {
                pbUserAvatar.Image = CreateDefaultAvatar(80, 80, CurrentUsername);
            }
        }

        private void BuildMainContentPanel()
        {
            mainContentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 242, 245),
                Padding = new Padding(40),
                AutoScroll = true
            };

            Panel welcomeSection = new Panel
            {
                Location = new Point(40, 40),
                Size = new Size(this.ClientSize.Width - 340, 90),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            welcomeSection.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = GetRoundedRectangle(welcomeSection.ClientRectangle, 12))
                {
                    e.Graphics.FillPath(new SolidBrush(Color.White), path);
                }
            };

            Label lblWelcome = new Label
            {
                Text = $"Welcome back, {(string.IsNullOrEmpty(CurrentUsername) ? "Guest" : CurrentUsername)}!",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 18, 80),
                Location = new Point(25, 18),
                AutoSize = true
            };

            Label lblSubtitle = new Label
            {
                Text = "Here's your profile information",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(27, 52),
                AutoSize = true
            };

            welcomeSection.Controls.Add(lblWelcome);
            welcomeSection.Controls.Add(lblSubtitle);

            userDetailPanel = new Panel
            {
                Location = new Point(40, 150),
                Size = new Size(this.ClientSize.Width - 340, 320),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            userDetailPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = GetRoundedRectangle(userDetailPanel.ClientRectangle, 12))
                {
                    e.Graphics.FillPath(new SolidBrush(Color.White), path);
                }
            };

            pbLargeAvatar = new PictureBox
            {
                Location = new Point(40, 30),
                Size = new Size(120, 120),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            System.Drawing.Drawing2D.GraphicsPath gp2 = new System.Drawing.Drawing2D.GraphicsPath();
            gp2.AddEllipse(0, 0, pbLargeAvatar.Width - 1, pbLargeAvatar.Height - 1);
            pbLargeAvatar.Region = new Region(gp2);
            pbLargeAvatar.Click += (s, e) => btnProfile_Click(s, e);

            // Display Name
            lblDisplayName = new Label
            {
                Text = CurrentUsername ?? "Guest User",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 18, 80),
                Location = new Point(180, 35),
                AutoSize = true
            };

            // Role Badge
            Label lblRoleBadge = new Label
            {
                Text = IsAdmin ? "👑 ADMIN" : "👤 USER",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = IsAdmin ? Color.FromArgb(255, 193, 7) : Color.FromArgb(0, 132, 255),
                Location = new Point(180, 70),
                Size = new Size(100, 28),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblRoleBadge.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Font symbolFont;
                try
                {
                    symbolFont = new Font("Segoe UI Symbol", lblRoleBadge.Font.Size, lblRoleBadge.Font.Style);
                }
                catch
                {
                    symbolFont = lblRoleBadge.Font;
                }

                using (var path = GetRoundedRectangle(lblRoleBadge.ClientRectangle, 5))
                {
                    e.Graphics.FillPath(new SolidBrush(lblRoleBadge.BackColor), path);

                    e.Graphics.DrawString(
                        lblRoleBadge.Text,
                        symbolFont,
                        new SolidBrush(lblRoleBadge.ForeColor),
                        lblRoleBadge.ClientRectangle,
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }

                if (symbolFont != lblRoleBadge.Font)
                {
                    symbolFont.Dispose();
                }
            };

            int startY = 115;
            int lineHeight = 38;

            // Email
            Label lblEmailIcon = new Label { Text = "📧", Font = new Font("Segoe UI", 14), Location = new Point(180, startY), Size = new Size(30, 30) };
            Label lblEmailTitle = new Label { Text = "Email:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(220, startY + 5), Size = new Size(80, 20) };
            lblUserEmail = new Label { Text = "Loading...", Font = new Font("Segoe UI", 10), ForeColor = Color.Black, Location = new Point(305, startY + 3), Size = new Size(userDetailPanel.Width - 330, 25), AutoEllipsis = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            // Phone
            Label lblPhoneIcon = new Label { Text = "📱", Font = new Font("Segoe UI", 14), Location = new Point(180, startY + lineHeight), Size = new Size(30, 30) };
            Label lblPhoneTitle = new Label { Text = "Phone:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(220, startY + lineHeight + 5), Size = new Size(80, 20) };
            lblUserPhone = new Label { Text = "Loading...", Font = new Font("Segoe UI", 10), ForeColor = Color.Black, Location = new Point(305, startY + lineHeight + 3), Size = new Size(userDetailPanel.Width - 330, 25), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            // Gender
            Label lblGenderIcon = new Label { Text = "👤", Font = new Font("Segoe UI", 14), Location = new Point(180, startY + lineHeight * 2), Size = new Size(30, 30) };
            Label lblGenderTitle = new Label { Text = "Gender:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(220, startY + lineHeight * 2 + 5), Size = new Size(80, 20) };
            lblUserGender = new Label { Text = "Loading...", Font = new Font("Segoe UI", 10), ForeColor = Color.Black, Location = new Point(305, startY + lineHeight * 2 + 3), Size = new Size(userDetailPanel.Width - 330, 25), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            // Member Since
            Label lblMemberIcon = new Label { Text = "📅", Font = new Font("Segoe UI", 14), Location = new Point(180, startY + lineHeight * 3), Size = new Size(30, 30) };
            Label lblMemberTitle = new Label { Text = "Member Since:", Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(220, startY + lineHeight * 3 + 5), Size = new Size(110, 20) };
            lblMemberSince = new Label { Text = "Loading...", Font = new Font("Segoe UI", 10), ForeColor = Color.Black, Location = new Point(335, startY + lineHeight * 3 + 3), Size = new Size(userDetailPanel.Width - 360, 25), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            // Buttons
            btnEditProfile = new Button
            {
                Text = "✏️ Edit Profile",
                Location = new Point(40, 260),
                Size = new Size(140, 42),
                BackColor = Color.FromArgb(0, 132, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditProfile.FlatAppearance.BorderSize = 0;
            btnEditProfile.Click += (s, e) => btnProfile_Click(s, e);

            btnViewDashboard = new Button
            {
                Text = "📊 View Dashboard",
                Location = new Point(190, 260),
                Size = new Size(160, 42),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnViewDashboard.FlatAppearance.BorderSize = 0;
            btnViewDashboard.Click += btnDashboard_Click;

            userDetailPanel.Controls.AddRange(new Control[]
            {
                pbLargeAvatar, lblDisplayName, lblRoleBadge,
                lblEmailIcon, lblEmailTitle, lblUserEmail,
                lblPhoneIcon, lblPhoneTitle, lblUserPhone,
                lblGenderIcon, lblGenderTitle, lblUserGender,
                lblMemberIcon, lblMemberTitle, lblMemberSince,
                btnEditProfile, btnViewDashboard
            });

            mainContentPanel.Controls.Add(welcomeSection);
            mainContentPanel.Controls.Add(userDetailPanel);

            this.Controls.Add(mainContentPanel);
            mainContentPanel.BringToFront();

            LoadUserProfileToMainPanel();
        }

        private async void LoadUserProfileToMainPanel()
        {
            if (string.IsNullOrEmpty(CurrentUsername))
            {
                lblDisplayName.Text = "Guest User";
                lblUserEmail.Text = "Not available";
                lblUserPhone.Text = "Not available";
                lblUserGender.Text = "Not specified";
                lblMemberSince.Text = "N/A";
                pbLargeAvatar.Image = CreateDefaultAvatar(120, 120, "?");
                return;
            }

            try
            {
                var user = await CrypTalk.FirebaseHelper.GetUser(CurrentUsername);

                if (user != null)
                {
                    lblDisplayName.Text = user.Username;
                    lblUserEmail.Text = string.IsNullOrEmpty(user.Email) ? "Not set" : user.Email;
                    lblUserPhone.Text = string.IsNullOrEmpty(user.Phone) ? "Not set" : user.Phone;
                    lblUserGender.Text = string.IsNullOrEmpty(user.Gender) ? "Not specified" : user.Gender;

                    if (!string.IsNullOrEmpty(user.CreatedAt))
                    {
                        DateTime.TryParse(user.CreatedAt, out DateTime createdAt);
                        lblMemberSince.Text = createdAt.ToString("MMMM dd, yyyy");
                    }
                    else
                    {
                        lblMemberSince.Text = DateTime.Now.ToString("MMMM dd, yyyy");
                    }

                    if (!string.IsNullOrEmpty(user.Avatar) && System.IO.File.Exists(user.Avatar))
                    {
                        pbLargeAvatar.Image = Image.FromFile(user.Avatar);
                    }
                    else
                    {
                        pbLargeAvatar.Image = CreateDefaultAvatar(120, 120, CurrentUsername);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile: {ex.Message}", "Error");
                lblUserEmail.Text = "Error loading data";
                lblUserPhone.Text = "Error loading data";
                lblUserGender.Text = "Error loading data";
            }
        }

        private Image CreateDefaultAvatar(int width, int height, string username)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, width, height),
                    Color.FromArgb(0, 132, 255),
                    Color.FromArgb(0, 100, 200),
                    45f))
                {
                    g.FillEllipse(brush, 0, 0, width, height);
                }

                string initial = username.Length > 0 ? username[0].ToString().ToUpper() : "?";
                using (Font font = new Font("Segoe UI", width / 2.5f, FontStyle.Bold))
                {
                    SizeF size = g.MeasureString(initial, font);
                    g.DrawString(initial, font, Brushes.White,
                        (width - size.Width) / 2,
                        (height - size.Height) / 2);
                }
            }
            return bmp;
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}