using CrypTalk;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Cryptalk
{
    public partial class ForgotPassword : Form
    {
        private string currentOTP = "";
        private DateTime otpExpireTime;
        private string verifiedUsername = "";

        public ForgotPassword()
        {
            InitializeComponent();

            txtNewPassword.Enabled = false;
            txtConfirmPassword.Enabled = false;
            btnForgotPassword.Text = "Send OTP";
        }

        private void btnForgotPassword_MouseEnter(object sender, EventArgs e)
        {
            btnForgotPassword.BackColor = Color.DodgerBlue;
        }

        private void btnForgotPassword_MouseLeave(object sender, EventArgs e)
        {
            btnForgotPassword.BackColor = Color.Transparent;
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

        private async void btnForgotPassword_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(verifiedUsername))
            {
                string username = txtUsernameForm.Text.Trim();

                if (string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Please enter your username!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnForgotPassword.Enabled = false;
                btnForgotPassword.Text = "Checking...";

                try
                {
                    var user = await FirebaseHelper.GetUser(username);

                    if (user == null)
                    {
                        MessageBox.Show("Username does not exist!", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(user.Email))
                    {
                        MessageBox.Show("You haven't set up your email!\n\nPlease update your profile first.",
                            "Email Required",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    currentOTP = EmailHelper.GenerateOTP();
                    otpExpireTime = DateTime.Now.AddMinutes(5);

                    btnForgotPassword.Text = "Sending OTP...";

                    bool sent = await EmailHelper.SendOTP(user.Email, username, currentOTP);

                    if (sent)
                    {
                        MessageBox.Show($"OTP has been sent to: {MaskEmail(user.Email)}\n\nPlease check your email!",
                            "OTP Sent",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        verifiedUsername = username;
                        txtUsernameForm.Enabled = false;
                        txtOTP.Enabled = true;
                        txtOTP.Focus();
                        btnForgotPassword.Text = "Verify OTP";
                    }
                    else
                    {
                        MessageBox.Show("Failed to send OTP. Please try again.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error");
                }
                finally
                {
                    btnForgotPassword.Enabled = true;
                }
            }
            else if (txtNewPassword.Enabled == false)
            {
                string enteredOTP = txtOTP.Text.Trim();

                if (string.IsNullOrEmpty(enteredOTP))
                {
                    MessageBox.Show("Please enter the OTP code!", "Error");
                    return;
                }

                if (DateTime.Now > otpExpireTime)
                {
                    MessageBox.Show("OTP has expired! Please request a new one.",
                        "Expired", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ResetForm();
                    return;
                }

                if (enteredOTP == currentOTP)
                {
                    MessageBox.Show("OTP verified successfully!\n\nYou can now set a new password.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtOTP.Enabled = false;
                    txtNewPassword.Enabled = true;
                    txtConfirmPassword.Enabled = true;
                    txtNewPassword.Focus();
                    btnForgotPassword.Text = "Reset Password";
                }
                else
                {
                    MessageBox.Show("❌ Invalid OTP code! Please try again.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                string newPassword = txtNewPassword.Text.Trim();
                string confirmPassword = txtConfirmPassword.Text.Trim();

                if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
                {
                    MessageBox.Show("Please enter both password fields!", "Error");
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match!", "Error");
                    return;
                }

                if (newPassword.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters!", "Error");
                    return;
                }

                btnForgotPassword.Enabled = false;
                btnForgotPassword.Text = "Changing...";

                try
                {
                    string hashedPassword = HashPassword(newPassword);

                    System.Diagnostics.Debug.WriteLine($"Username: {verifiedUsername}");
                    System.Diagnostics.Debug.WriteLine($"Hashed Password Length: {hashedPassword.Length}");

                    bool success = await FirebaseHelper.ChangePassword(verifiedUsername, hashedPassword);

                    if (success)
                    {
                        MessageBox.Show("Password changed successfully!\n\nPlease log in with your new password.",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Login l = new Login();
                        l.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to change password!\n\nUsername: {verifiedUsername}\nPassword length: {newPassword.Length} characters\n\nPlease check:\n- Firebase connection\n- User exists in database\n- Database permissions",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Error occurred:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                        "Detailed Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    btnForgotPassword.Enabled = true;
                    btnForgotPassword.Text = "Reset Password";
                }
            }
        }

        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                return email;

            string[] parts = email.Split('@');
            string username = parts[0];
            string domain = parts[1];

            if (username.Length <= 2)
                return email;

            string masked = username[0] + new string('*', username.Length - 2) + username[username.Length - 1];
            return masked + "@" + domain;
        }

        private void ResetForm()
        {
            verifiedUsername = "";
            currentOTP = "";
            txtUsernameForm.Enabled = true;
            txtOTP.Enabled = false;
            txtNewPassword.Enabled = false;
            txtConfirmPassword.Enabled = false;
            txtUsernameForm.Clear();
            txtOTP.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
            btnForgotPassword.Text = "Send OTP";
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