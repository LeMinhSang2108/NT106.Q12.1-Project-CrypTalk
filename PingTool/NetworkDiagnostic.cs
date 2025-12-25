using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace PingTool 
{
    public partial class NetworkDiagnostic : Form
    {
        private TextBox txtHost;
        private NumericUpDown numPort, numInterval;
        private Button btnAddHost, btnRemoveHost, btnStartPing, btnStop;
        private Button btnPortCheck, btnTraceroute, btnClearLog, btnSaveLog;
        private DataGridView dgvHosts;
        private RichTextBox rtbResults;
        private Label lblStatus, lblStats;
        private ProgressBar progressBar;
        private Panel pnlHeader, pnlControl, pnlGrid, pnlResults;

        private CancellationTokenSource cts;
        private bool isRunning = false;
        private DataTable dtHosts;

        private Dictionary<string, HostStats> hostStatistics = new Dictionary<string, HostStats>();

        public NetworkDiagnostic()
        {
            InitializeComponent();
            BuildUI();
            InitializeDataTable();
        }

        private void InitializeDataTable()
        {
            dtHosts = new DataTable();
            dtHosts.Columns.Add("STT", typeof(int));
            dtHosts.Columns.Add("Hostname", typeof(string));
            dtHosts.Columns.Add("IP", typeof(string));
            dtHosts.Columns.Add("Latency", typeof(string));
            dtHosts.Columns.Add("Status", typeof(string));
            dtHosts.Columns.Add("PacketLoss", typeof(string));
            dtHosts.Columns.Add("LastCheck", typeof(string));
            dtHosts.Columns.Add("Port", typeof(string));

            dgvHosts.DataSource = dtHosts;

            dgvHosts.Columns["STT"].Width = 50;
            dgvHosts.Columns["Hostname"].Width = 180;
            dgvHosts.Columns["IP"].Width = 120;
            dgvHosts.Columns["Latency"].Width = 90;
            dgvHosts.Columns["Status"].Width = 100;
            dgvHosts.Columns["PacketLoss"].Width = 90;
            dgvHosts.Columns["LastCheck"].Width = 120;
            dgvHosts.Columns["Port"].Width = 100;

            dgvHosts.Columns["Port"].HeaderText = "Port";
        }


        private void BuildUI()
        {
            this.Text = "Network Diagnostic Tool - Cryptalk";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.Font = new Font("Segoe UI", 9F);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            pnlControl = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(1050, 150),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblHost = new Label { Text = "Enter domain name:", Location = new Point(20, 15), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), AutoSize = true };
            txtHost = new TextBox { Location = new Point(20, 40), Size = new Size(280, 28), Font = new Font("Segoe UI", 10F) };
            btnAddHost = new Button { Text = "➕ Thêm", Location = new Point(310, 40), Size = new Size(90, 28), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnAddHost.FlatAppearance.BorderSize = 0;

            Label lblPort = new Label { Text = "Port:", Location = new Point(430, 15), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), AutoSize = true };
            numPort = new NumericUpDown { Location = new Point(430, 40), Size = new Size(80, 28), Minimum = 1, Maximum = 65535, Value = 80, Font = new Font("Segoe UI", 10F) };

            Label lblInterval = new Label { Text = "Interval (s):", Location = new Point(540, 15), Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), AutoSize = true };
            numInterval = new NumericUpDown { Location = new Point(540, 40), Size = new Size(80, 28), Minimum = 1, Maximum = 60, Value = 2, Font = new Font("Segoe UI", 10F) };

            btnStartPing = new Button { Text = "▶ Start Ping", Location = new Point(20, 85), Size = new Size(140, 32), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnStartPing.FlatAppearance.BorderSize = 0;

            btnStop = new Button { Text = "⏹ Stop", Location = new Point(170, 85), Size = new Size(110, 32), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnStop.FlatAppearance.BorderSize = 0;

            btnRemoveHost = new Button { Text = "🗑 Remove Selected", Location = new Point(290, 85), Size = new Size(140, 32), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnRemoveHost.FlatAppearance.BorderSize = 0;

            btnPortCheck = new Button { Text = "🔌 Check Port", Location = new Point(450, 85), Size = new Size(140, 32), BackColor = Color.FromArgb(23, 162, 184), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnPortCheck.FlatAppearance.BorderSize = 0;

            btnTraceroute = new Button { Text = "🗺 Traceroute", Location = new Point(600, 85), Size = new Size(130, 32), BackColor = Color.FromArgb(255, 152, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnTraceroute.FlatAppearance.BorderSize = 0;

            btnClearLog = new Button { Text = "🧹 Clear Log", Location = new Point(740, 85), Size = new Size(110, 32), BackColor = Color.FromArgb(255, 193, 7), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnClearLog.FlatAppearance.BorderSize = 0;

            btnSaveLog = new Button { Text = "💾 Save Log", Location = new Point(860, 85), Size = new Size(110, 32), BackColor = Color.FromArgb(111, 66, 193), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSaveLog.FlatAppearance.BorderSize = 0;

            pnlControl.Controls.AddRange(new Control[] { lblHost, txtHost, btnAddHost, lblPort, numPort, lblInterval, numInterval,
        btnStartPing, btnStop, btnRemoveHost, btnPortCheck, btnTraceroute, btnClearLog, btnSaveLog });

            Panel pnlStatus = new Panel { Location = new Point(20, 180), Size = new Size(1050, 50), BackColor = Color.FromArgb(248, 249, 250), BorderStyle = BorderStyle.FixedSingle };
            lblStatus = new Label { Text = "⏳ Ready", Location = new Point(20, 15), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(108, 117, 125), AutoSize = true };
            lblStats = new Label { Text = "Hosts: 0 | Online: 0 | Offline: 0", Location = new Point(350, 15), Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(73, 80, 87), AutoSize = true };
            progressBar = new ProgressBar { Location = new Point(750, 15), Size = new Size(280, 20), Style = ProgressBarStyle.Marquee, Visible = false };
            pnlStatus.Controls.AddRange(new Control[] { lblStatus, lblStats, progressBar });

            pnlGrid = new Panel { Location = new Point(20, 240), Size = new Size(1050, 200), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblGridTitle = new Label { Text = "📋 HOST LIST", Location = new Point(10, 5), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(10, 18, 80), AutoSize = true };
            dgvHosts = new DataGridView
            {
                Location = new Point(10, 35),
                Size = new Size(1030, 155),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 35,
                RowHeadersVisible = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                EnableHeadersVisualStyles = false,
                Font = new Font("Segoe UI", 9F)
            };
            dgvHosts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(10, 18, 80);
            dgvHosts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvHosts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgvHosts.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHosts.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvHosts.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvHosts.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvHosts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgvHosts.RowTemplate.Height = 28;
            pnlGrid.Controls.AddRange(new Control[] { lblGridTitle, dgvHosts });

            pnlResults = new Panel { Location = new Point(20, 450), Size = new Size(1050, 200), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Label lblResultsTitle = new Label { Text = "📊 DETAILED RESULTS", Location = new Point(10, 5), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(10, 18, 80), AutoSize = true };
            rtbResults = new RichTextBox { Location = new Point(10, 35), Size = new Size(1030, 155), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(0, 255, 127), Font = new Font("Consolas", 9F), ReadOnly = true, BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.Vertical };
            pnlResults.Controls.AddRange(new Control[] { lblResultsTitle, rtbResults });

            this.Controls.AddRange(new Control[] { pnlControl, pnlStatus, pnlGrid, pnlResults });

            btnAddHost.Click += BtnAddHost_Click;
            btnRemoveHost.Click += BtnRemoveHost_Click;
            btnStartPing.Click += BtnStartPing_Click;
            btnStop.Click += BtnStop_Click;
            btnPortCheck.Click += BtnPortCheck_Click;
            btnTraceroute.Click += BtnTraceroute_Click;
            btnClearLog.Click += (s, e) => rtbResults.Clear();
            btnSaveLog.Click += BtnSaveLog_Click;
            txtHost.KeyPress += (s, e) => { if (e.KeyChar == (char)13) { e.Handled = true; BtnAddHost_Click(s, e); } };
            dgvHosts.CellFormatting += DgvHosts_CellFormatting;
        }


        private void DgvHosts_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvHosts.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status.Contains("Success") || status.Contains("Online"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(40, 167, 69);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else if (status.Contains("Timeout") || status.Contains("Offline"))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(220, 53, 69);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
            }
        }

        private void BtnAddHost_Click(object sender, EventArgs e)
        {
            string host = txtHost.Text.Trim();
            if (string.IsNullOrEmpty(host))
            {
                MessageBox.Show("Please enter a domain name or IP address!", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (DataRow row in dtHosts.Rows)
            {
                if (row["Hostname"].ToString() == host)
                {
                    MessageBox.Show("This host already exists in the list!", "Notification",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            dtHosts.Rows.Add(dtHosts.Rows.Count + 1, host, "---", "---", "Đang chờ", "---", "---");
            hostStatistics[host] = new HostStats();
            txtHost.Clear();
            txtHost.Focus();

            UpdateStatsLabel();
            AppendColoredText($"[{DateTime.Now:HH:mm:ss}] ➕ Added host: {host}\n", Color.Cyan);
        }

        private void BtnRemoveHost_Click(object sender, EventArgs e)
        {
            if (dgvHosts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a host to remove!", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isRunning)
            {
                MessageBox.Show("Please stop pinging before removing a host!", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hostname = dgvHosts.SelectedRows[0].Cells["Hostname"].Value.ToString();
            dtHosts.Rows.RemoveAt(dgvHosts.SelectedRows[0].Index);
            hostStatistics.Remove(hostname);

            for (int i = 0; i < dtHosts.Rows.Count; i++)
            {
                dtHosts.Rows[i]["STT"] = i + 1;
            }

            UpdateStatsLabel();
            AppendColoredText($"[{DateTime.Now:HH:mm:ss}] 🗑 Deleted host: {hostname}\n", Color.OrangeRed);
        }

        private async void BtnStartPing_Click(object sender, EventArgs e)
        {
            if (dgvHosts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please choose at least one host to ping!", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StartOperation();
            cts = new CancellationTokenSource();
            int interval = (int)numInterval.Value * 1000;

            AppendColoredText($"\n{'=',-60}\n", Color.Gray);
            AppendColoredText($"▶ START PING - Cycle: {numInterval.Value}s\n", Color.Cyan);
            AppendColoredText($"{'=',-60}\n\n", Color.Gray);

            try
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var tasks = new List<Task>();

                    foreach (DataGridViewRow selectedRow in dgvHosts.SelectedRows)
                    {
                        string hostname = selectedRow.Cells["Hostname"].Value.ToString();
                        int rowIndex = selectedRow.Index;

                        tasks.Add(Task.Run(async () =>
                        {
                            await PingHost(hostname, rowIndex);
                        }));
                    }

                    await Task.WhenAll(tasks);
                    UpdateStatsLabel();

                    await Task.Delay(interval, cts.Token);
                }
            }
            catch (TaskCanceledException)
            {
                AppendColoredText($"\n⏹ Ping stopped by the user.\n\n", Color.Yellow);
            }
            finally
            {
                StopOperation();
            }
        }


        private async Task PingHost(string hostname, int rowIndex)
        {
            int port = (int)numPort.Value;
            HostStats stats = hostStatistics[hostname];
            stats.TotalPings++;

            using (Ping pinger = new Ping())
            {
                try
                {
                    var reply = await pinger.SendPingAsync(hostname, 3000);

                    string portStatus = "---";

                    if (reply.Status == IPStatus.Success)
                    {
                        stats.SuccessPings++;

                        try
                        {
                            using (TcpClient client = new TcpClient())
                            {
                                var task = client.ConnectAsync(hostname, port);
                                var timeoutTask = Task.Delay(2000);
                                var finishedTask = await Task.WhenAny(task, timeoutTask);
                                portStatus = (finishedTask == task && client.Connected) ? "Open" : "Closed";
                            }
                        }
                        catch
                        {
                            portStatus = "Closed";
                        }

                        stats.Latencies.Add(reply.RoundtripTime);

                        dgvHosts.Invoke(new Action(() =>
                        {
                            dtHosts.Rows[rowIndex]["IP"] = reply.Address.ToString();
                            dtHosts.Rows[rowIndex]["Latency"] = $"{reply.RoundtripTime} ms";
                            dtHosts.Rows[rowIndex]["Status"] = "✅ Success";
                            dtHosts.Rows[rowIndex]["PacketLoss"] = $"{stats.PacketLossRate:F1}%";
                            dtHosts.Rows[rowIndex]["LastCheck"] = DateTime.Now.ToString("HH:mm:ss");
                            dtHosts.Rows[rowIndex]["Port"] = portStatus;
                        }));

                        AppendColoredText($"[{DateTime.Now:HH:mm:ss}] ✅ {hostname,-25} → {reply.Address,-15} " +
                                          $"Time={reply.RoundtripTime,3}ms Port {port}:{portStatus}\n", Color.LightGreen);
                    }
                    else
                    {
                        stats.FailedPings++;

                        dgvHosts.Invoke(new Action(() =>
                        {
                            dtHosts.Rows[rowIndex]["Status"] = $"❌ {reply.Status}";
                            dtHosts.Rows[rowIndex]["PacketLoss"] = $"{stats.PacketLossRate:F1}%";
                            dtHosts.Rows[rowIndex]["LastCheck"] = DateTime.Now.ToString("HH:mm:ss");
                            dtHosts.Rows[rowIndex]["Port"] = "N/A";
                        }));

                        AppendColoredText($"[{DateTime.Now:HH:mm:ss}] ❌ {hostname,-25} → {reply.Status}\n", Color.OrangeRed);
                    }
                }
                catch (Exception ex)
                {
                    stats.FailedPings++;

                    dgvHosts.Invoke(new Action(() =>
                    {
                        dtHosts.Rows[rowIndex]["Status"] = "❌ Error";
                        dtHosts.Rows[rowIndex]["LastCheck"] = DateTime.Now.ToString("HH:mm:ss");
                        dtHosts.Rows[rowIndex]["Port"] = "Error";
                    }));

                    AppendColoredText($"[{DateTime.Now:HH:mm:ss}] ⚠ {hostname,-25} → Error: {ex.Message}\n", Color.Red);
                }
            }
        }




        private async void BtnPortCheck_Click(object sender, EventArgs e)
        {
            if (dgvHosts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please choose a host to check the port!", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string hostname = dgvHosts.SelectedRows[0].Cells["Hostname"].Value.ToString();
            int port = (int)numPort.Value;

            AppendColoredText($"\n▶ Check port {port} on {hostname}...\n", Color.Cyan);

            try
            {
                var sw = Stopwatch.StartNew();
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(hostname, port);
                    sw.Stop();

                    if (client.Connected)
                    {
                        AppendColoredText($"✅ Port {port} OPEN - Connection time: {sw.ElapsedMilliseconds}ms\n\n", Color.LightGreen);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendColoredText($"❌ Port {port} closing or unable to connect\n", Color.Red);
                AppendColoredText($"   Error: {ex.Message}\n\n", Color.Gray);
            }
        }

        private async void BtnTraceroute_Click(object sender, EventArgs e)
        {
            if (dgvHosts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please choose a host to traceroute!", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StartOperation();
            cts = new CancellationTokenSource();

            string hostname = dgvHosts.SelectedRows[0].Cells["Hostname"].Value.ToString();

            AppendColoredText($"\n{'=',-60}\n", Color.Gray);
            AppendColoredText($"🗺 TRACEROUTE đến {hostname}\n", Color.Cyan);
            AppendColoredText($"{'=',-60}\n\n", Color.Gray);

            try
            {
                using (Ping pinger = new Ping())
                {
                    for (int ttl = 1; ttl <= 30; ttl++)
                    {
                        if (cts.Token.IsCancellationRequested) break;

                        PingOptions options = new PingOptions(ttl, true);
                        byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

                        var reply = await pinger.SendPingAsync(hostname, 5000, buffer, options);

                        if (reply.Status == IPStatus.Success)
                        {
                            AppendColoredText($" {ttl,2}  {reply.RoundtripTime,4} ms  {reply.Address}\n", Color.LightGreen);
                            AppendColoredText($"     ✅ Reached the destination!\n\n", Color.Cyan);
                            break;
                        }
                        else if (reply.Status == IPStatus.TtlExpired)
                        {
                            AppendColoredText($" {ttl,2}  {reply.RoundtripTime,4} ms  {reply.Address}\n", Color.Yellow);
                        }
                        else if (reply.Status == IPStatus.TimedOut)
                        {
                            AppendColoredText($" {ttl,2}      *       Request timed out\n", Color.Gray);
                        }

                        await Task.Delay(200, cts.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                AppendColoredText("\n⏹ Traceroute has been stopped.\n\n", Color.Yellow);
            }
            finally
            {
                StopOperation();
            }
        }

        private void BtnSaveLog_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rtbResults.Text))
            {
                MessageBox.Show("No log to save!", "Notification",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text Files (*.txt)|*.txt|Log Files (*.log)|*.log|All Files (*.*)|*.*";
                sfd.FileName = $"NetworkDiag_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                sfd.Title = "Save Log";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, rtbResults.Text);
                        MessageBox.Show($"Log saved successfully!\n\n{sfd.FileName}", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        AppendColoredText($"[{DateTime.Now:HH:mm:ss}] 💾 Log saved to: {sfd.FileName}\n", Color.Cyan);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file:\n{ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            cts?.Cancel();
        }

        private void StartOperation()
        {
            isRunning = true;
            btnStartPing.Enabled = false;
            btnStop.Enabled = true;
            btnAddHost.Enabled = false;
            btnRemoveHost.Enabled = false;
            txtHost.Enabled = false;
            numInterval.Enabled = false;
            progressBar.Visible = true;
            lblStatus.Text = "⚡ Running...";
            lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
        }

        private void StopOperation()
        {
            isRunning = false;
            btnStartPing.Enabled = true;
            btnStop.Enabled = false;
            btnAddHost.Enabled = true;
            btnRemoveHost.Enabled = true;
            txtHost.Enabled = true;
            numInterval.Enabled = true;
            progressBar.Visible = false;
            lblStatus.Text = "✅ Completed";
            lblStatus.ForeColor = Color.FromArgb(40, 167, 69);
        }

        private void UpdateStatsLabel()
        {
            int total = dtHosts.Rows.Count;
            int active = 0;
            int failed = 0;

            foreach (DataRow row in dtHosts.Rows)
            {
                string status = row["Status"].ToString();
                if (status.Contains("Success") || status.Contains("Online"))
                    active++;
                else if (status.Contains("Timeout") || status.Contains("Error") || status.Contains("Offline"))
                    failed++;
            }

            lblStats.Text = $"Hosts: {total} | Online: {active} | Offline: {failed}";
        }

        private void AppendColoredText(string text, Color color)
        {
            if (rtbResults.InvokeRequired)
            {
                rtbResults.Invoke(new Action(() => AppendColoredText(text, color)));
                return;
            }

            rtbResults.SelectionStart = rtbResults.TextLength;
            rtbResults.SelectionLength = 0;
            rtbResults.SelectionColor = color;
            rtbResults.AppendText(text);
            rtbResults.SelectionColor = rtbResults.ForeColor;
            rtbResults.ScrollToCaret();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cts?.Cancel();
            base.OnFormClosing(e);
        }

        private class HostStats
        {
            public int TotalPings { get; set; }
            public int SuccessPings { get; set; }
            public int FailedPings { get; set; }
            public List<long> Latencies { get; set; } = new List<long>();

            public double PacketLossRate => TotalPings > 0 ? (FailedPings * 100.0 / TotalPings) : 0;
            public double AverageLatency => Latencies.Count > 0 ? Latencies.Average() : 0;
        }
    }
}