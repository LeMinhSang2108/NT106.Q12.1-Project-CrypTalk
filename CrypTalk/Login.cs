using System;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using Cryptalk;

namespace CrypTalk
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void llblRegister_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Register r = new Register();
            r.ShowDialog();
        }

        private void llblForgotPassword_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ForgotPassword f = new ForgotPassword();
            f.ShowDialog();
        }

        private void btnLogin_MouseEnter_1(object sender, EventArgs e)
        {
            btnLogin.BackColor = Color.DodgerBlue;
        }

        private void btnLogin_MouseLeave(object sender, EventArgs e)
        {
            btnLogin.BackColor = Color.Transparent;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsernameForm.Text.Trim();
            string password = txtPasswordForm.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your username and password!");
                return;
            }

            string hashedPassword = HashPassword(password);

            btnLogin.Enabled = false;
            btnLogin.Text = "Logging in...";
            btnLogin.ForeColor = Color.White;
            btnLogin.Font = new Font(btnLogin.Font, FontStyle.Bold);

            try
            {
                var user = await FirebaseHelper.Login(username, hashedPassword);

                if (user != null)
                {
                    string role = user.IsAdmin ? "ADMIN" : "USER";
                    MessageBox.Show($"✅ Login successful!\n\n👤 Username: {username}\n🔑 Role: {role}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Menu m = new Menu();
                    m.CurrentUsername = username;
                    m.IsAdmin = user.IsAdmin;
                    this.Hide();
                    m.ShowDialog();
                    this.txtPasswordForm.Clear();
                    this.Show();
                    this.txtPasswordForm.Focus();
                }
                else
                {
                    MessageBox.Show("❌ Username or password is incorrect!",
                        "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}");
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                 "Do you want to exit?",
                 "Confirm exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Close();
            }
        }
    }
}