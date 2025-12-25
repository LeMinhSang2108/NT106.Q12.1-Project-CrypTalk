using System;
using System.Windows.Forms;

namespace CrypTalk
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                FirebaseHelper.Initialize();

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new CrypTalk.Login());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Application startup failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}