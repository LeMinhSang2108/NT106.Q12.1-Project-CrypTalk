using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Cryptalk
{
    public partial class Dashboard : Form
    {
        private string currentUsername;
        private DateTime lastLogin;
        private Label lblTotalMessages;
        private Label lblFriends;
        private Label lblToday;

        public Dashboard()
        {
            InitializeComponent();
        }

        public Dashboard(string username, DateTime loginTime)
        {
            InitializeComponent();
            currentUsername = username;
            lastLogin = loginTime;
            BuildUI();
            UpdateStats();
        }

        private void BuildUI()
        {
            this.Text = "User Dashboard";
            this.Size = new Size(730, 600);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 18, 80);

            Panel plHeader = new Panel
            {
                BackColor = Color.FromArgb(10, 18, 80),
                Dock = DockStyle.Top,
                Height = 60
            };

            Label lblHeader = new Label
            {
                Text = "USER DASHBOARD",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            Button btnClose = new Button
            {
                Text = "✕",
                BackColor = Color.FromArgb(10, 18, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 40),
                Location = new Point(660, 10),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            btnClose.MouseEnter += (s, e) => btnClose.BackColor = Color.FromArgb(192, 0, 0);
            btnClose.MouseLeave += (s, e) => btnClose.BackColor = Color.FromArgb(10, 18, 80);

            plHeader.Controls.Add(lblHeader);
            plHeader.Controls.Add(btnClose);

            Panel plContent = new Panel
            {
                BackColor = Color.White,
                Location = new Point(40, 90),
                Size = new Size(640, 460),
                BorderStyle = BorderStyle.None
            };

            Label lblWelcome = new Label
            {
                Text = $"Welcome back, {currentUsername}!",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 18, 80),
                Location = new Point(30, 30),
                AutoSize = true
            };

            Label lblLastLogin = new Label
            {
                Text = $"Last login: {lastLogin:dd/MM/yyyy hh:mm tt}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(34, 65),
                AutoSize = true
            };

            Panel separator1 = new Panel
            {
                BackColor = Color.FromArgb(220, 220, 220),
                Location = new Point(20, 105),
                Size = new Size(600, 2)
            };

            Label lblStatsHeader = new Label
            {
                Text = "Activity Statistics",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 18, 80),
                Location = new Point(30, 130),
                AutoSize = true
            };

            Panel separator2 = new Panel
            {
                BackColor = Color.FromArgb(220, 220, 220),
                Location = new Point(20, 170),
                Size = new Size(600, 2)
            };

            Panel plStats = new Panel
            {
                BackColor = Color.FromArgb(245, 247, 250),
                Location = new Point(20, 190),
                Size = new Size(600, 180),
                BorderStyle = BorderStyle.None
            };

            lblTotalMessages = new Label
            {
                Text = "• Total messages: ...",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(40, 35),
                AutoSize = true
            };

            lblFriends = new Label
            {
                Text = "• Number of friends: ...",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(40, 80),
                AutoSize = true
            };

            lblToday = new Label
            {
                Text = "• Today's message: ...",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location = new Point(40, 125),
                AutoSize = true
            };

            plStats.Controls.Add(lblTotalMessages);
            plStats.Controls.Add(lblFriends);
            plStats.Controls.Add(lblToday);
            plContent.Controls.Add(lblWelcome);
            plContent.Controls.Add(lblLastLogin);
            plContent.Controls.Add(separator1);
            plContent.Controls.Add(lblStatsHeader);
            plContent.Controls.Add(separator2);
            plContent.Controls.Add(plStats);

            this.Controls.Add(plContent);
            this.Controls.Add(plHeader);
        }

        private void UpdateStats()
        {
            try
            {
                var (total, friends, today, debugMsg) = GetUserStatsFromFile();

                lblTotalMessages.Text = $"• Total messages: {total}";
                lblFriends.Text = $"• Number of friends: {friends}";
                lblToday.Text = $"• Today's message: {today}";

                lblTotalMessages.ForeColor = total > 0 ? Color.FromArgb(0, 102, 204) : Color.Gray;
                lblFriends.ForeColor = friends > 0 ? Color.FromArgb(0, 153, 51) : Color.Gray;
                lblToday.ForeColor = today > 0 ? Color.FromArgb(204, 102, 0) : Color.Gray;
            }
            catch (Exception ex)
            {
                lblTotalMessages.Text = $"• Total messages: 0";
                lblFriends.Text = $"• Number of friends: 0";
                lblToday.Text = $"• Today's message: 0";

                MessageBox.Show($"Could not load statistics: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (int totalMessages, int friends, int todayMessages, string debugMsg) GetUserStatsFromFile()
        {
            string folder = Path.Combine(Application.StartupPath, "data", "chat_logs");
            string path = Path.Combine(folder, $"{currentUsername}.json");

            string debugMsg = $"Username: {currentUsername}\n";
            debugMsg += $"Looking for file: {path}\n";
            debugMsg += $"File exists: {File.Exists(path)}\n";

            if (!Directory.Exists(folder))
            {
                debugMsg += "Folder does NOT exist! Creating...\n";
                Directory.CreateDirectory(folder);
            }
            else
            {
                debugMsg += "Folder exists ✓\n";
            }

            if (!File.Exists(path))
            {
                debugMsg += "File NOT found! No messages yet.";
                return (0, 0, 0, debugMsg);
            }

            try
            {
                string json = File.ReadAllText(path);
                debugMsg += $"File size: {json.Length} bytes\n";

                var logs = JsonSerializer.Deserialize<List<ChatMessage>>(json);

                if (logs == null || logs.Count == 0)
                {
                    debugMsg += "File is empty or invalid JSON";
                    return (0, 0, 0, debugMsg);
                }

                debugMsg += $"Total messages in file: {logs.Count}\n";

                int total = logs.Count;
                int today = logs.Count(m => m.Timestamp.Date == DateTime.Now.Date);

                debugMsg += $"Today's date: {DateTime.Now.Date:dd/MM/yyyy}\n";
                debugMsg += $"Messages today: {today}\n";

                var friendsList = logs
                    .Select(m => m.Sender == currentUsername ? m.Receiver : m.Sender)
                    .Where(name => !string.IsNullOrEmpty(name) && name != currentUsername && name != "ALL")
                    .Distinct()
                    .ToList();

                int friends = friendsList.Count;
                debugMsg += $"Friends list: {string.Join(", ", friendsList)}";

                return (total, friends, today, debugMsg);
            }
            catch (Exception ex)
            {
                debugMsg += $"Error reading file: {ex.Message}";
                return (0, 0, 0, debugMsg);
            }
        }

        public class ChatMessage
        {
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public string Message { get; set; }
            public string ContentType { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}