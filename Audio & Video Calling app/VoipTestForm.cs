using System;
using System.Collections.Generic;
using System.Linq;
using SWF = System.Windows.Forms;
using SD = System.Drawing;
using SP = ScottPlot;

namespace Audio___Video_Calling_app
{
    public partial class VoipTestForm : SWF.Form
    {
        private VoipClient voipClient;

        private SWF.TextBox remoteIpBox;
        private SWF.NumericUpDown remotePortBox, localPortBox;
        private SWF.Button startCallBtn, stopCallBtn;
        private SWF.CheckBox jitterBufferCheckbox, echoCancellerCheckbox, noiseSuppressionCheckbox;
        private SWF.Button startRecordBtn, stopRecordBtn, viewRecordingsBtn;
        private SWF.ComboBox networkConditionCombo;
        private SWF.RichTextBox logBox;

        private SWF.Label statsLabel;
        private SP.WinForms.FormsPlot jitterChart, packetLossChart;

        private Queue<double> jitterHistory = new Queue<double>(50);
        private Queue<double> lossRateHistory = new Queue<double>(50);

        public VoipTestForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "VoIP Test - Advanced Features";
            Size = new SD.Size(1100, 750);

            var connPanel = new SWF.GroupBox
            {
                Text = "🌐 Connection",
                Location = new SD.Point(10, 10),
                Size = new SD.Size(250, 120),
                Font = new SD.Font(this.Font, SD.FontStyle.Bold)
            };

            connPanel.Controls.AddRange(new SWF.Control[]
            {
                new SWF.Label { Text = "Local Port:", Location = new SD.Point(10, 25), AutoSize = true },
                localPortBox = new SWF.NumericUpDown
                {
                    Location = new SD.Point(100, 22), Width = 80,
                    Minimum = 1000, Maximum = 65535, Value = 5001
                },
                new SWF.Label { Text = "Remote IP:", Location = new SD.Point(10, 55), AutoSize = true },
                remoteIpBox = new SWF.TextBox { Location = new SD.Point(100, 52), Width = 130, Text = "127.0.0.1" },
                new SWF.Label { Text = "Remote Port:", Location = new SD.Point(10, 85), AutoSize = true },
                remotePortBox = new SWF.NumericUpDown
                {
                    Location = new SD.Point(112, 82), Width = 80,
                    Minimum = 1000, Maximum = 65535, Value = 5002
                }
            });

            var callPanel = new SWF.GroupBox
            {
                Text = "📞 Call Controls",
                Location = new SD.Point(270, 10),
                Size = new SD.Size(250, 120),
                Font = new SD.Font(this.Font, SD.FontStyle.Bold)
            };

            startCallBtn = new SWF.Button
            {
                Text = "▶ Start Call",
                Location = new SD.Point(10, 25),
                Size = new SD.Size(110, 35),
                BackColor = System.Drawing.Color.LightGreen,
                Font = new SD.Font(this.Font, SD.FontStyle.Bold)
            };

            stopCallBtn = new SWF.Button
            {
                Text = "⬛ Stop Call",
                Location = new SD.Point(130, 25),
                Size = new SD.Size(110, 35),
                BackColor = System.Drawing.Color.LightCoral,
                Enabled = false,
                Font = new SD.Font(this.Font, SD.FontStyle.Bold)
            };

            jitterBufferCheckbox = new SWF.CheckBox
            {
                Text = "Jitter Buffer",
                Location = new SD.Point(10, 70),
                Width = 110,
                Checked = true
            };

            echoCancellerCheckbox = new SWF.CheckBox
            {
                Text = "Echo Cancel",
                Location = new SD.Point(130, 70),
                Width = 110,
                Checked = false
            };

            noiseSuppressionCheckbox = new SWF.CheckBox
            {
                Text = "Noise Filter",
                Location = new SD.Point(10, 95),
                Width = 110,
                Checked = false
            };

            callPanel.Controls.AddRange(new SWF.Control[]
            {
                startCallBtn, stopCallBtn, jitterBufferCheckbox, echoCancellerCheckbox, noiseSuppressionCheckbox
            });

            var recPanel = new SWF.GroupBox
            {
                Text = "🔴 Recording",
                Location = new SD.Point(530, 10),
                Size = new SD.Size(250, 120),
                Font = new SD.Font(this.Font, SD.FontStyle.Bold)
            };

            startRecordBtn = new SWF.Button
            {
                Text = "● Start Rec",
                Location = new SD.Point(10, 25),
                Size = new SD.Size(110, 35),
                BackColor = SD.Color.FromArgb(255, 200, 200),
                Enabled = false
            };

            stopRecordBtn = new SWF.Button
            {
                Text = "⏹ Stop Rec",
                Location = new SD.Point(130, 25),
                Size = new SD.Size(110, 35),
                BackColor = SD.Color.FromArgb(200, 200, 200),
                Enabled = false
            };

            viewRecordingsBtn = new SWF.Button
            {
                Text = "📁 View Recordings",
                Location = new SD.Point(10, 70),
                Size = new SD.Size(230, 30),
                BackColor = SD.Color.LightBlue
            };

            recPanel.Controls.AddRange(new SWF.Control[]
            {
                startRecordBtn, stopRecordBtn, viewRecordingsBtn
            });

            var netSimPanel = new SWF.GroupBox
            {
                Text = "🌐 Network Simulator",
                Location = new SD.Point(790, 10),
                Size = new SD.Size(280, 120),
                Font = new SD.Font(this.Font, SD.FontStyle.Bold)
            };

            var netLabel = new SWF.Label
            {
                Text = "Network Condition:",
                Location = new SD.Point(10, 30),
                AutoSize = true
            };

            networkConditionCombo = new SWF.ComboBox
            {
                Location = new SD.Point(10, 55),
                Width = 260,
                DropDownStyle = SWF.ComboBoxStyle.DropDownList
            };
            networkConditionCombo.Items.AddRange(new object[]
            {
                "Perfect (No simulation)",
                "Good (0.5% loss, 20ms±5)",
                "Fair (2% loss, 50ms±15)",
                "Poor (5% loss, 100ms±30)",
                "Very Poor (10% loss, 200ms±50)"
            });
            networkConditionCombo.SelectedIndex = 0;

            netSimPanel.Controls.AddRange(new SWF.Control[] { netLabel, networkConditionCombo });

            statsLabel = new SWF.Label
            {
                Location = new SD.Point(10, 140),
                Size = new SD.Size(1060, 100),
                BorderStyle = SWF.BorderStyle.FixedSingle,
                Font = new SD.Font("Consolas", 9),
                BackColor = SD.Color.FromArgb(245, 245, 245),
                Padding = new SWF.Padding(10)
            };
            UpdateStatsDisplay(null);

            jitterChart = CreateChart("Jitter (ms)", new SD.Point(10, 250), SD.Color.Blue);
            packetLossChart = CreateChart("Packet Loss (%)", new SD.Point(545, 250), SD.Color.Red);

            logBox = new SWF.RichTextBox
            {
                Location = new SD.Point(10, 530),
                Size = new SD.Size(1060, 180),
                ReadOnly = true,
                BackColor = SD.Color.FromArgb(30, 30, 30),
                ForeColor = SD.Color.LightGreen,
                Font = new SD.Font("Consolas", 9)
            };

            Controls.AddRange(new SWF.Control[]
            {
                connPanel, callPanel, recPanel, netSimPanel,
                statsLabel, jitterChart, packetLossChart, logBox
            });

            startCallBtn.Click += (s, e) => StartCall();
            stopCallBtn.Click += (s, e) => StopCall();
            startRecordBtn.Click += (s, e) => StartRecording();
            stopRecordBtn.Click += (s, e) => StopRecording();
            viewRecordingsBtn.Click += (s, e) => ViewRecordings();

            jitterBufferCheckbox.CheckedChanged += (s, e) =>
            {
                if (voipClient != null)
                {
                    voipClient.IsJitterBufferEnabled = jitterBufferCheckbox.Checked;
                    LogMessage($"⏱ Jitter Buffer: {(jitterBufferCheckbox.Checked ? "ON" : "OFF")}",
                               jitterBufferCheckbox.Checked ? SD.Color.Green : SD.Color.Orange);
                }
            };

            echoCancellerCheckbox.CheckedChanged += (s, e) =>
            {
                if (voipClient != null)
                {
                    voipClient.IsEchoCancellerEnabled = echoCancellerCheckbox.Checked;
                    LogMessage($"🔊 Echo Canceller: {(echoCancellerCheckbox.Checked ? "ON" : "OFF")}",
                               echoCancellerCheckbox.Checked ? SD.Color.Green : SD.Color.Gray);
                }
            };

            noiseSuppressionCheckbox.CheckedChanged += (s, e) =>
            {
                if (voipClient != null)
                {
                    voipClient.IsNoiseSuppressorEnabled = noiseSuppressionCheckbox.Checked;
                    LogMessage($"🎙 Noise Suppressor: {(noiseSuppressionCheckbox.Checked ? "ON" : "OFF")}",
                               noiseSuppressionCheckbox.Checked ? SD.Color.Cyan : SD.Color.Gray);
                }
            };

            networkConditionCombo.SelectedIndexChanged += (s, e) =>
            {
                if (voipClient != null)
                {
                    var condition = (NetworkCondition)networkConditionCombo.SelectedIndex;
                    voipClient.NetworkSim.ApplyPreset(condition);

                    string msg = condition == NetworkCondition.Perfect
                        ? "🌐 Network: Perfect (No delay/loss)"
                        : $"🌐 Network: {networkConditionCombo.Text}";

                    LogMessage(msg, condition == NetworkCondition.Perfect ? SD.Color.White : SD.Color.Yellow);
                }
            };
        }

        private SP.WinForms.FormsPlot CreateChart(string title, SD.Point location, SD.Color lineColor)
        {
            var chart = new SP.WinForms.FormsPlot
            {
                Location = location,
                Size = new SD.Size(520, 270)
            };

            chart.Plot.Title(title);
            chart.Plot.XLabel("Time (samples)");
            chart.Plot.YLabel(title);
            chart.Plot.Axes.SetLimits(0, 50, 0, 100);

            return chart;
        }

        private void StartCall()
        {
            try
            {
                voipClient = new VoipClient((int)localPortBox.Value);

                voipClient.OnStatsUpdated += stats =>
                {
                    if (InvokeRequired)
                        Invoke(new Action(() =>
                        {
                            UpdateStatsDisplay(stats);
                            UpdateCharts(stats);
                        }));
                };

                voipClient.OnLog += msg =>
                {
                    if (InvokeRequired)
                        Invoke(new Action(() => LogMessage(msg, SD.Color.White)));
                };

                voipClient.IsJitterBufferEnabled = jitterBufferCheckbox.Checked;
                voipClient.IsEchoCancellerEnabled = echoCancellerCheckbox.Checked;
                voipClient.IsNoiseSuppressorEnabled = noiseSuppressionCheckbox.Checked;
                voipClient.NetworkSim.ApplyPreset((NetworkCondition)networkConditionCombo.SelectedIndex);

                voipClient.StartCall(remoteIpBox.Text, (int)remotePortBox.Value);

                startCallBtn.Enabled = false;
                stopCallBtn.Enabled = true;
                startRecordBtn.Enabled = true;

                LogMessage("=== CALL STARTED ===", SD.Color.Cyan);
            }
            catch (Exception ex)
            {
                SWF.MessageBox.Show($"Error: {ex.Message}", "Error", SWF.MessageBoxButtons.OK, SWF.MessageBoxIcon.Error);
            }
        }

        private void StopCall()
        {
            if (voipClient?.IsRecording == true)
                StopRecording();

            voipClient?.StopCall();
            voipClient?.Dispose();
            voipClient = null;

            startCallBtn.Enabled = true;
            stopCallBtn.Enabled = false;
            startRecordBtn.Enabled = false;
            stopRecordBtn.Enabled = false;

            LogMessage("=== CALL ENDED ===", SD.Color.Cyan);
        }

        private void StartRecording()
        {
            voipClient?.StartRecording();
            startRecordBtn.Enabled = false;
            stopRecordBtn.Enabled = true;
        }

        private void StopRecording()
        {
            string filePath = voipClient?.StopRecording();
            startRecordBtn.Enabled = true;
            stopRecordBtn.Enabled = false;

            if (!string.IsNullOrEmpty(filePath))
            {
                var result = SWF.MessageBox.Show(
                    $"Recording saved!\n\n{filePath}\n\nPlay now?",
                    "Recording Saved",
                    SWF.MessageBoxButtons.YesNo,
                    SWF.MessageBoxIcon.Information);

                if (result == SWF.DialogResult.Yes)
                {
                    voipClient?.Recorder.PlayRecording(filePath);
                }
            }
        }

        private void ViewRecordings()
        {
            var recordingsForm = new SWF.Form
            {
                Text = "Recordings",
                Size = new SD.Size(600, 400),
                StartPosition = SWF.FormStartPosition.CenterParent
            };

            var listView = new SWF.ListView
            {
                Dock = SWF.DockStyle.Fill,
                View = SWF.View.Details,
                FullRowSelect = true
            };
            listView.Columns.Add("File", 200);
            listView.Columns.Add("Duration", 80);
            listView.Columns.Add("Size", 80);
            listView.Columns.Add("Date", 150);

            var recordings = voipClient?.Recorder.GetRecordings() ?? new string[0];
            foreach (var file in recordings)
            {
                var info = voipClient.Recorder.GetRecordingInfo(file);
                if (info != null)
                {
                    var item = new SWF.ListViewItem(new[]
                    {
                        info.FileName,
                        info.FormattedDuration,
                        info.FormattedFileSize,
                        info.CreatedDate.ToString("g")
                    });
                    item.Tag = info.FilePath;
                    listView.Items.Add(item);
                }
            }

            listView.DoubleClick += (s, e) =>
            {
                if (listView.SelectedItems.Count > 0)
                {
                    string path = listView.SelectedItems[0].Tag.ToString();
                    voipClient?.Recorder.PlayRecording(path);
                }
            };

            recordingsForm.Controls.Add(listView);
            recordingsForm.ShowDialog();
        }

        private void UpdateStatsDisplay(JitterBufferStats stats)
        {
            if (stats == null)
            {
                statsLabel.Text = "📊 Waiting for call...";
                return;
            }

            string noiseStats = "";
            if (voipClient?.NoiseSuppressor != null)
            {
                noiseStats = $"  |  🔇 Noise: {voipClient.NoiseSuppressor.NoiseLevel:F4}";
            }

            string echoStats = "";
            if (voipClient?.EchoCanceller != null)
            {
                echoStats = $"🔊 Echo: {voipClient.EchoCanceller.EchoSuppressionLevel:F1}%";
            }

            statsLabel.Text =
                $"📊 STATISTICS\n" +
                $"📥 Received: {stats.PacketsReceived}  |  " +
                $"▶ Played: {stats.PacketsPlayed}  |  " +
                $"❌ Lost: {stats.PacketsLost} ({stats.PacketLossRate:F1}%)\n" +
                $"⏱ Jitter: {stats.AverageJitter:F1}ms  |  " +
                $"🔄 Buffer Adjusts: {stats.BufferAdjustments}" +
                (string.IsNullOrEmpty(echoStats) ? "" : $"  |  {echoStats}") +
                noiseStats;
        }

        private void UpdateCharts(JitterBufferStats stats)
        {
            jitterHistory.Enqueue(stats.AverageJitter);
            if (jitterHistory.Count > 50) jitterHistory.Dequeue();

            double[] xData = jitterHistory.Select((v, i) => (double)i).ToArray();
            double[] yData = jitterHistory.ToArray();

            jitterChart.Plot.Clear();
            var scatter1 = jitterChart.Plot.Add.Scatter(xData, yData);
            scatter1.Color = SP.Color.FromColor(SD.Color.Blue);
            scatter1.LineWidth = 2;
            jitterChart.Plot.Axes.SetLimits(0, 50, 0, Math.Max(50, yData.Length > 0 ? yData.Max() * 1.2 : 50));
            jitterChart.Refresh();

            double currentLossRate = (stats.PacketsReceived > 0) 
                ? (stats.PacketsLost * 100.0 / stats.PacketsReceived) : 0.0;

            lossRateHistory.Enqueue(currentLossRate);
            if (lossRateHistory.Count > 50) lossRateHistory.Dequeue();

            double[] lossX = lossRateHistory.Select((v, i) => (double)i).ToArray();
            double[] lossY = lossRateHistory.ToArray();

            packetLossChart.Plot.Clear();
            var scatter2 = packetLossChart.Plot.Add.Scatter(lossX, lossY);
            scatter2.Color = SP.Color.FromColor(SD.Color.Red);
            scatter2.LineWidth = 2;
            packetLossChart.Plot.Axes.SetLimits(0, 50, 0, Math.Max(10, lossY.Length > 0 ? lossY.Max() * 1.2 : 10));
            packetLossChart.Refresh();
        }

        private void LogMessage(string msg, SD.Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogMessage(msg, color)));
                return;
            }

            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionColor = color;
            logBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
            logBox.ScrollToCaret();
        }

        protected override void OnFormClosing(SWF.FormClosingEventArgs e)
        {
            StopCall();
            base.OnFormClosing(e);
        }
    }
}