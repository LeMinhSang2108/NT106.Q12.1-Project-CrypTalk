using CrypTalk;
using Firebase.Database.Query;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cryptalk
{
    public partial class Profile : Form
    {
        private string currentUsername;
        private PictureBox pbAvatar;
        private Label lblUsername;
        private Label lblUserID;
        private Label lblMemberSince;
        private Button btnChangeAvatar;
        private Button btnSaveAvatar;
        private Button btnEditProfile;
        private Button btnClose;
        private Panel plProfile;
        private Panel plHeader;
        private string selectedImagePath = "";

        public Profile(string username)
        {
            BuildUI();
            currentUsername = username;
            LoadUserProfile();
            this.FormClosing += Profile_FormClosing;
        }

        private void BuildUI()
        {
            this.Text = "User Profile";
            this.Size = new Size(500, 600);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 18, 80);

            plHeader = new Panel
            {
                BackColor = Color.FromArgb(10, 18, 80),
                Dock = DockStyle.Top,
                Height = 40
            };

            Label lblTitle = new Label
            {
                Text = "CrypTalk - Profile",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            btnClose = new Button
            {
                Text = "✕",
                BackColor = Color.FromArgb(10, 18, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(460, 5),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += BtnClose_Click;

            plHeader.Controls.Add(lblTitle);
            plHeader.Controls.Add(btnClose);

            plProfile = new Panel
            {
                BackColor = Color.FromArgb(192, 192, 192),
                Location = new Point(50, 80),
                Size = new Size(400, 480),
                BorderStyle = BorderStyle.None
            };

            pbAvatar = new PictureBox
            {
                Location = new Point(125, 30),
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pbAvatar.Width - 1, pbAvatar.Height - 1);
            pbAvatar.Region = new Region(gp);
            pbAvatar.Image = CreateDefaultAvatar(150, 150);

            lblUsername = new Label
            {
                Location = new Point(50, 200),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblUserID = new Label
            {
                Location = new Point(50, 240),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            lblMemberSince = new Label
            {
                Location = new Point(50, 270),
                Size = new Size(300, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            Color blueColor = Color.FromArgb(10, 18, 80);

            btnChangeAvatar = new Button
            {
                Text = "Change Avatar",
                Location = new Point(50, 330),
                Size = new Size(300, 40),
                BackColor = blueColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnChangeAvatar.FlatAppearance.BorderSize = 0;
            btnChangeAvatar.Click += BtnChangeAvatar_Click;

            btnSaveAvatar = new Button
            {
                Text = "Save Avatar",
                Location = new Point(50, 380),
                Size = new Size(300, 40),
                BackColor = blueColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Enabled = false,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnSaveAvatar.FlatAppearance.BorderSize = 0;
            btnSaveAvatar.Click += BtnSaveAvatar_Click;

            btnEditProfile = new Button
            {
                Text = "Edit Profile",
                Location = new Point(50, 430),
                Size = new Size(300, 40),
                BackColor = blueColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEditProfile.FlatAppearance.BorderSize = 0;
            btnEditProfile.Click += BtnEditProfile_Click;

            plProfile.Controls.Add(pbAvatar);
            plProfile.Controls.Add(lblUsername);
            plProfile.Controls.Add(lblUserID);
            plProfile.Controls.Add(lblMemberSince);
            plProfile.Controls.Add(btnChangeAvatar);
            plProfile.Controls.Add(btnSaveAvatar);
            plProfile.Controls.Add(btnEditProfile);

            this.Controls.Add(plProfile);
            this.Controls.Add(plHeader);
        }


        private async void LoadUserProfile()
        {
            try
            {
                var user = await FirebaseHelper.GetUser(currentUsername);

                if (user != null)
                {
                    lblUsername.Text = user.Username;
                    lblUserID.Text = $"User ID: {user.Username}";

                    if (!string.IsNullOrEmpty(user.CreatedAt))
                    {
                        DateTime.TryParse(user.CreatedAt, out DateTime createdAt);
                        lblMemberSince.Text = $"Member since: {createdAt:MMM yyyy}";
                    }
                    else
                    {
                        lblMemberSince.Text = $"Member since: {DateTime.Now:MMM yyyy}";
                    }

                    if (!string.IsNullOrEmpty(user.Avatar) && File.Exists(user.Avatar))
                    {
                        pbAvatar.Image = Image.FromFile(user.Avatar);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile: {ex.Message}");
            }
        }

        private void BtnChangeAvatar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                ofd.Title = "Select Avatar Image";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(ofd.FileName);
                        if (fileInfo.Length > 2 * 1024 * 1024)
                        {
                            MessageBox.Show("❌ Avatar must be less than 2MB!",
                                "File Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        pbAvatar.Image = Image.FromFile(ofd.FileName);
                        selectedImagePath = ofd.FileName;
                        btnSaveAvatar.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}");
                    }
                }
            }
        }

        private async void BtnSaveAvatar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedImagePath))
            {
                MessageBox.Show("Please select an avatar first!");
                return;
            }

            string appPath = Application.StartupPath;
            string avatarsFolder = Path.Combine(appPath, "Avatars");

            if (!Directory.Exists(avatarsFolder))
                Directory.CreateDirectory(avatarsFolder);

            string fileName = currentUsername + "_" + Path.GetFileName(selectedImagePath);
            string destPath = Path.Combine(avatarsFolder, fileName);

            try
            {
                btnSaveAvatar.Enabled = false;
                btnSaveAvatar.Text = "Saving...";

                File.Copy(selectedImagePath, destPath, true);

                var user = await FirebaseHelper.GetUser(currentUsername);
                if (user != null)
                {
                    user.Avatar = destPath;

                    await FirebaseHelper.firebaseClient
                        .Child("users")
                        .Child(currentUsername)
                        .PutAsync(user);

                    MessageBox.Show("✅ Avatar updated successfully!", "Success");
                    selectedImagePath = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving avatar: {ex.Message}", "Error");
                btnSaveAvatar.Enabled = true;
            }
            finally
            {
                btnSaveAvatar.Text = "Save Avatar";
            }
        }

        private void BtnEditProfile_Click(object sender, EventArgs e)
        {
            EditProfileForm editForm = new EditProfileForm(currentUsername);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadUserProfile();
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private Image CreateDefaultAvatar(int width, int height)
        {
            try
            {
                string assetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
                string imagePath = Path.Combine(assetsDir, "avatar.jpg");

                if (File.Exists(imagePath))
                {
                    Image originalImage = Image.FromFile(imagePath);
                    Bitmap resizedImage = new Bitmap(width, height);

                    using (Graphics g = Graphics.FromImage(resizedImage))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(originalImage, 0, 0, width, height);
                    }

                    originalImage.Dispose();
                    return resizedImage;
                }
            }
            catch { }

            Bitmap placeholder = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.Clear(Color.LightGray);
                using (Font font = new Font("Segoe UI", 12, FontStyle.Bold))
                {
                    string text = "No Image";
                    SizeF size = g.MeasureString(text, font);
                    g.DrawString(text, font, Brushes.White,
                        (width - size.Width) / 2,
                        (height - size.Height) / 2);
                }
            }
            return placeholder;
        }

        private void Profile_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedImagePath) && btnSaveAvatar.Enabled)
            {
                DialogResult result = MessageBox.Show(
                    "Do you want to discard the change?",
                    "Unsaved Avatar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    pbAvatar.Image = CreateDefaultAvatar(150, 150);
                    selectedImagePath = "";
                    btnSaveAvatar.Enabled = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        public partial class EditProfileForm : Form
        {
            private string currentUsername;
            private TextBox txtEmail;
            private TextBox txtPhone;
            private ComboBox cboGender;
            private Button btnSave;
            private Button btnCancel;

            public EditProfileForm(string username)
            {
                currentUsername = username;
                BuildEditUI();
                LoadProfileData();
            }

            private void BuildEditUI()
            {
                this.Text = "Edit Profile";
                this.Size = new Size(400, 380);
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.BackColor = Color.White;

                Label lblTitle = new Label
                {
                    Text = "Edit Profile Information",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Location = new Point(20, 20),
                    AutoSize = true
                };

                Label lblEmail = new Label
                {
                    Text = "Email:",
                    Location = new Point(20, 70),
                    AutoSize = true
                };

                txtEmail = new TextBox
                {
                    Location = new Point(20, 95),
                    Size = new Size(340, 25)
                };

                Label lblPhone = new Label
                {
                    Text = "Phone:",
                    Location = new Point(20, 130),
                    AutoSize = true
                };

                txtPhone = new TextBox
                {
                    Location = new Point(20, 155),
                    Size = new Size(340, 25)
                };

                Label lblGender = new Label
                {
                    Text = "Gender:",
                    Location = new Point(20, 190),
                    AutoSize = true
                };

                cboGender = new ComboBox
                {
                    Location = new Point(20, 215),
                    Size = new Size(340, 25),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cboGender.Items.AddRange(new string[] { "Male", "Female", "Other" });

                btnSave = new Button
                {
                    Text = "Save",
                    Location = new Point(180, 270),
                    Size = new Size(80, 35),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnSave.FlatAppearance.BorderSize = 0;
                btnSave.Click += BtnSave_Click;

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(280, 270),
                    Size = new Size(80, 35),
                    BackColor = Color.Gray,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

                this.Controls.AddRange(new Control[] {
                    lblTitle, lblEmail, txtEmail, lblPhone, txtPhone,
                    lblGender, cboGender, btnSave, btnCancel
                });
            }

            private async void LoadProfileData()
            {
                try
                {
                    var user = await FirebaseHelper.GetUser(currentUsername);

                    if (user != null)
                    {
                        txtEmail.Text = user.Email ?? "";
                        txtPhone.Text = user.Phone ?? "";

                        if (!string.IsNullOrEmpty(user.Gender))
                            cboGender.SelectedItem = user.Gender;
                    }
                }
                catch { }
            }

            private async void BtnSave_Click(object sender, EventArgs e)
            {
                try
                {
                    btnSave.Enabled = false;
                    btnSave.Text = "Saving...";

                    bool success = await FirebaseHelper.UpdateProfile(
                        currentUsername,
                        txtEmail.Text.Trim(),
                        txtPhone.Text.Trim(),
                        cboGender.SelectedItem?.ToString() ?? ""
                    );

                    if (success)
                    {
                        MessageBox.Show("✅ Profile updated!", "Success");
                        this.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show("❌ Failed to update profile!", "Error");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error");
                }
                finally
                {
                    btnSave.Enabled = true;
                    btnSave.Text = "Save";
                }
            }
        }
    }
}