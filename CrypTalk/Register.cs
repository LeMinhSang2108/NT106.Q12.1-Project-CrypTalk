using CrypTalk;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Cryptalk
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
        }

        private void btnRegister_MouseEnter(object sender, EventArgs e)
        {
            btnRegister.BackColor = Color.DodgerBlue;
        }

        private void btnRegister_MouseLeave(object sender, EventArgs e)
        {
            btnRegister.BackColor = Color.Transparent;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsernameForm.Text.Trim();
            string password = txtPasswordForm.Text.Trim();
            string confirm = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
            {
                MessageBox.Show("Please enter all the information!");
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Password and Confirm Password do not match!");
                return;
            }

            string hashedPassword = HashPassword(password);
            btnRegister.Enabled = false;
            btnRegister.Text = "Registering...";

            try
            {
                bool success = await FirebaseHelper.RegisterUser(username, hashedPassword);

                if (success)
                {
                    MessageBox.Show("✅ Registration successful!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    CrypTalk.Login l = new CrypTalk.Login();
                    l.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("❌ Registration failed! Username may already exist.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "Register";
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