using Audio___Video_Calling_app;
using System;
using System.Windows.Forms;

namespace VoipApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VoipTestForm());
        }
    }
}