using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using CrypTalk;

namespace Cryptalk
{
    public partial class AdminPanel : Form
    {
        private DataGridView dgvUsers;
        private Button btnRefresh;
        private Button btnDeleteUser;
        private Label lblTotal;

        public AdminPanel()
        {
            InitializeComponent();
            BuildUI();
            LoadUsers();
        }

        private void BuildUI()
        {
            this.Text = "Admin Panel - User Management";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(10, 18, 80)
            };

            Label lblTitle = new Label
            {
                Text = "👑 ADMIN PANEL",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            headerPanel.Controls.Add(lblTitle);

            // Toolbar
            Panel toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(10)
            };

            btnRefresh = new Button
            {
                Text = "🔄 Refresh",
                Location = new Point(10, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 132, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => await LoadUsers();

            btnDeleteUser = new Button
            {
                Text = "🗑️ Delete User",
                Location = new Point(120, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnDeleteUser.FlatAppearance.BorderSize = 0;
            btnDeleteUser.Click += async (s, e) => await DeleteSelectedUser();

            lblTotal = new Label
            {
                Text = "Total Users: 0",
                Location = new Point(700, 15),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(10, 18, 80)
            };

            toolbarPanel.Controls.AddRange(new Control[] { btnRefresh, btnDeleteUser, lblTotal });

            // DataGridView
            dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9)
            };

            // Style header
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(10, 18, 80);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);

            // Style rows
            dgvUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            dgvUsers.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 132, 255);
            dgvUsers.RowsDefaultCellStyle.SelectionForeColor = Color.White;

            dgvUsers.SelectionChanged += (s, e) =>
            {
                btnDeleteUser.Enabled = dgvUsers.SelectedRows.Count > 0;
            };

            // Add columns
            dgvUsers.Columns.Add("Username", "Username");
            dgvUsers.Columns.Add("Email", "Email");
            dgvUsers.Columns.Add("Phone", "Phone");
            dgvUsers.Columns.Add("Gender", "Gender");
            dgvUsers.Columns.Add("IsAdmin", "Role");
            dgvUsers.Columns.Add("CreatedAt", "Created At");
            dgvUsers.Columns.Add("LastLogin", "Last Login");

            this.Controls.Add(dgvUsers);
            this.Controls.Add(toolbarPanel);
            this.Controls.Add(headerPanel);
        }

        private async System.Threading.Tasks.Task LoadUsers()
        {
            try
            {
                btnRefresh.Enabled = false;
                btnRefresh.Text = "Loading...";

                dgvUsers.Rows.Clear();

                var users = await FirebaseHelper.GetAllUsers();

                foreach (var user in users.OrderBy(u => u.Username))
                {
                    string role = user.IsAdmin ? "👑 ADMIN" : "👤 USER";

                    dgvUsers.Rows.Add(
                        user.Username,
                        user.Email ?? "N/A",
                        user.Phone ?? "N/A",
                        user.Gender ?? "N/A",
                        role,
                        user.CreatedAt ?? "N/A",
                        user.LastLogin ?? "Never"
                    );
                }

                lblTotal.Text = $"Total Users: {users.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
                btnRefresh.Text = "🔄 Refresh";
            }
        }

        private async System.Threading.Tasks.Task DeleteSelectedUser()
        {
            if (dgvUsers.SelectedRows.Count == 0) return;

            string username = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();

            var result = MessageBox.Show(
                $"⚠️ Are you sure you want to delete user '{username}'?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = await FirebaseHelper.DeleteUser(username);

                    if (success)
                    {
                        MessageBox.Show($"✅ User '{username}' deleted successfully!", "Success");
                        await LoadUsers();
                    }
                    else
                    {
                        MessageBox.Show($"❌ Failed to delete user '{username}'", "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}