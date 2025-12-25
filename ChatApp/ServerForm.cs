using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
namespace ChatApp
{
    public partial class ServerForm : Form
    {
        private ChatServer server;
        private RichTextBox logBox;
        private Button startBtn;
        private Button stopBtn;
        private NumericUpDown portInput;
        private Label statsLabel;
        private Process videoServerProcess;

        private int messageCount = 0;

        public ServerForm()
        {
            Text = "Chat Server - Message Monitor";
            Size = new Size(700, 500);

            var portLabel = new Label { Text = "Port:", Location = new Point(10, 15), AutoSize = true };
            portInput = new NumericUpDown
            {
                Location = new Point(50, 12),
                Width = 80,
                Minimum = 1000,
                Maximum = 65535,
                Value = 5000,
                Enabled = false
            };

            startBtn = new Button { Text = "Start Server", Location = new Point(140, 10), Width = 100, Height = 30 };
            stopBtn = new Button { Text = "Stop Server", Location = new Point(250, 10), Width = 100, Height = 30, Enabled = false };

            statsLabel = new Label
            {
                Text = "Status: Stopped | Messages: 0",
                Location = new Point(360, 15),
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font(this.Font, FontStyle.Bold)
            };

            logBox = new RichTextBox
            {
                Location = new Point(10, 50),
                Size = new Size(660, 410),
                ScrollBars = RichTextBoxScrollBars.Vertical,
                ReadOnly = true,
                BackColor = Color.White,
                Font = new Font("Consolas", 9)
            };

            Controls.AddRange(new Control[] { portLabel, portInput, startBtn, stopBtn, statsLabel, logBox });

            startBtn.Click += (s, e) => StartServer();
            stopBtn.Click += (s, e) => StopServer();
        }

        private void StartServer()
        {
            server = new ChatServer();

            server.OnLog += (log) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() => AppendColoredLog(log)));
                else
                    AppendColoredLog(log);
            };

            server.OnMessageForwarded += (sender, receiver, contentType) =>
            {
                if (InvokeRequired)
                    Invoke(new Action(() =>
                    {
                        messageCount++;
                        UpdateStats();
                    }));
                else
                {
                    messageCount++;
                    UpdateStats();
                }
            };

            server.Start((int)portInput.Value);

            StartVideoServerProcess();

            startBtn.Enabled = false;
            stopBtn.Enabled = true;
            portInput.Enabled = false;

            UpdateStats();
            AppendColoredLog("🚀 Server ready to accept connections...");
            AppendColoredLog("🎥 Video Server process started in background.");
        }

        private void StopServer()
        {
            server?.Stop();

            StopVideoServerProcess();

            startBtn.Enabled = true;
            stopBtn.Enabled = false;
            portInput.Enabled = true;

            UpdateStats();
            AppendColoredLog("🛑 Server stopped.");
        }

        private void StartVideoServerProcess()
        {
            try
            {
                string currentFolder = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(currentFolder, "video_server.py");

                if (!File.Exists(scriptPath))
                {
                    AppendColoredLog("❌ Error: video_server.py not found!");
                    return;
                }

                // Kill process cũ nếu lỡ còn sót lại
                StopVideoServerProcess();

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "python";
                start.Arguments = $"\"{scriptPath}\"";

                // Cấu hình để chạy ẩn hoàn toàn (No Window)
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.WorkingDirectory = currentFolder;

                videoServerProcess = new Process();
                videoServerProcess.StartInfo = start;
                videoServerProcess.Start();
            }
            catch (Exception ex)
            {
                AppendColoredLog($"❌ Failed to start Video Server: {ex.Message}");
            }
        }

        private void StopVideoServerProcess()
        {
            try
            {
                if (videoServerProcess != null && !videoServerProcess.HasExited)
                {
                    videoServerProcess.Kill();
                    videoServerProcess.Dispose();
                    videoServerProcess = null;
                }

                // Dọn dẹp mạnh tay hơn (optional): Kill tất cả python đang chạy script này
                // (Chỉ nên dùng nếu cách trên không sạch)
            }
            catch { }
        }

        private void UpdateStats()
        {
            string status = stopBtn.Enabled ? "🟢 Running" : "🔴 Stopped";
            Color statusColor = stopBtn.Enabled ? Color.Green : Color.Red;

            statsLabel.Text = $"{status} | Messages: {messageCount}";
            statsLabel.ForeColor = statusColor;
        }

        private void AppendColoredLog(string log)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");

            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionColor = Color.Gray;
            logBox.SelectionFont = new Font(logBox.Font, FontStyle.Regular);
            logBox.AppendText($"[{timestamp}] ");

            if (log.Contains("✅") && log.Contains("connected"))
            {
                logBox.SelectionColor = Color.Green;
                logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                logBox.AppendText(log + "\n");
            }
            else if (log.Contains("❌") && log.Contains("disconnected"))
            {
                logBox.SelectionColor = Color.Red;
                logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                logBox.AppendText(log + "\n");
            }
            else if (log.Contains("📤") || log.Contains("📢"))
            {
                string[] parts = log.Split(new[] { " → " }, StringSplitOptions.None);
                if (parts.Length >= 2)
                {
                    if (log.Contains("📢"))
                    {
                        logBox.SelectionColor = Color.DarkOrange;
                        logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                        logBox.AppendText("📢 BROADCAST ");
                    }
                    else
                    {
                        logBox.SelectionColor = Color.DarkBlue;
                        logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                        logBox.AppendText("📤 PRIVATE ");
                    }

                    string sender = parts[0].Replace("📤", "").Replace("📢", "").Trim();
                    logBox.SelectionColor = Color.Blue;
                    logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                    logBox.AppendText($"{sender}");

                    logBox.SelectionColor = Color.Gray;
                    logBox.SelectionFont = new Font(logBox.Font, FontStyle.Regular);
                    logBox.AppendText(" → ");

                    string[] receiverInfo = parts[1].Split(new[] { ": " }, StringSplitOptions.None);
                    logBox.SelectionColor = Color.DarkGreen;
                    logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                    logBox.AppendText(receiverInfo[0].Replace("(", "").Replace(")", "").Split(' ')[0]);

                    if (receiverInfo.Length > 1)
                    {
                        logBox.SelectionColor = Color.Gray;
                        logBox.SelectionFont = new Font(logBox.Font, FontStyle.Regular);
                        logBox.AppendText(" | ");

                        string contentType = receiverInfo[1];
                        if (contentType.Contains("📷") || contentType.Contains("Image"))
                        {
                            logBox.SelectionColor = Color.Purple;
                            logBox.AppendText("📷 IMAGE");
                        }
                        else if (contentType.Contains("🎵") || contentType.Contains("Audio"))
                        {
                            logBox.SelectionColor = Color.DeepPink;
                            logBox.AppendText("🎵 AUDIO");
                        }
                        else
                        {
                            logBox.SelectionColor = Color.Black;
                            logBox.AppendText("💬 TEXT");
                        }
                    }

                    logBox.AppendText("\n");
                }
            }
            else if (log.Contains("Server started"))
            {
                logBox.SelectionColor = Color.DarkGreen;
                logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                logBox.AppendText(log + "\n");
            }
            else if (log.Contains("Server stopped"))
            {
                logBox.SelectionColor = Color.DarkRed;
                logBox.SelectionFont = new Font(logBox.Font, FontStyle.Bold);
                logBox.AppendText(log + "\n");
            }
            else
            {
                logBox.SelectionColor = Color.Black;
                logBox.SelectionFont = new Font(logBox.Font, FontStyle.Regular);
                logBox.AppendText(log + "\n");
            }

            logBox.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            server?.Stop();
            StopVideoServerProcess();
            base.OnFormClosing(e);
        }
    }
}