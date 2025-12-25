using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public class AIAssistantDialog : Form
    {
        private TabControl tabControl;

        private TabPage tabSmartReply;
        private TextBox txtContext;
        private Button btnGenerateReply;
        private RichTextBox txtReplyResult;
        private ComboBox cmbTone;

        // Translation tab
        private TabPage tabTranslation;
        private TextBox txtTranslateInput;
        private ComboBox cmbSourceLang;
        private ComboBox cmbTargetLang;
        private Button btnTranslate;
        private RichTextBox txtTranslateResult;

        // Search tab
        private TabPage tabSearch;
        private TextBox txtSearchQuery;
        private Button btnSearch;
        private ListBox lstSearchResults;
        private Label lblSearchCount;

        // Summarization tab
        private TabPage tabSummarize;
        private Button btnLoadConversation;
        private RichTextBox txtSummaryResult;
        private ComboBox cmbSummaryLength;
        private Button btnGenerateSummary;

        // Data
        private List<ChatMessage> allMessages;
        private string currentUsername;
        private string currentTarget;

        public string SelectedReply { get; private set; }
        public string TranslatedText { get; private set; }

        public AIAssistantDialog(string username, string target, List<ChatMessage> messages)
        {
            this.currentUsername = username;
            this.currentTarget = target;
            this.allMessages = messages ?? new List<ChatMessage>();

            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "🤖 BOT";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(10, 18, 80)
            };

            Label lblTitle = new Label
            {
                Text = "🤖 BOT ASSISTANT",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            headerPanel.Controls.Add(lblTitle);

            tabControl = new TabControl
            {
                Location = new Point(10, 70),
                Size = new Size(670, 480),
                Font = new Font("Segoe UI", 10)
            };

            BuildSmartReplyTab();
            BuildTranslationTab();
            BuildSearchTab();
            BuildSummarizationTab();

            this.Controls.Add(headerPanel);
            this.Controls.Add(tabControl);
        }

        #region Smart Reply Tab

        private void BuildSmartReplyTab()
        {
            tabSmartReply = new TabPage("💬 Smart Reply");

            Label lblInstruction = new Label
            {
                Text = "Enter recent conversation context to get BOT-suggested replies:",
                Location = new Point(20, 20),
                Size = new Size(600, 20),
                Font = new Font("Segoe UI", 9)
            };

            Label lblContext = new Label
            {
                Text = "Context (last messages):",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtContext = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(620, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9)
            };

            LoadRecentContext();

            Label lblTone = new Label
            {
                Text = "Reply Tone:",
                Location = new Point(20, 185),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            cmbTone = new ComboBox
            {
                Location = new Point(120, 182),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTone.Items.AddRange(new object[] {
                "Friendly",
                "Professional",
                "Casual",
                "Formal",
                "Humorous"
            });
            cmbTone.SelectedIndex = 0;

            btnGenerateReply = new Button
            {
                Text = "✨ Generate Smart Reply",
                Location = new Point(20, 220),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 132, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGenerateReply.FlatAppearance.BorderSize = 0;
            btnGenerateReply.Click += BtnGenerateReply_Click;

            Label lblResult = new Label
            {
                Text = "Suggested Replies:",
                Location = new Point(20, 270),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtReplyResult = new RichTextBox
            {
                Location = new Point(20, 295),
                Size = new Size(620, 120),
                ReadOnly = true,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Button btnUseReply = new Button
            {
                Text = "📋 Use This Reply",
                Location = new Point(480, 220),
                Size = new Size(160, 40),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnUseReply.FlatAppearance.BorderSize = 0;
            btnUseReply.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtReplyResult.Text))
                {
                    SelectedReply = txtReplyResult.Text;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            tabSmartReply.Controls.AddRange(new Control[]
            {
                lblInstruction, lblContext, txtContext,
                lblTone, cmbTone, btnGenerateReply,
                lblResult, txtReplyResult, btnUseReply
            });

            tabControl.TabPages.Add(tabSmartReply);
        }

        private void LoadRecentContext()
        {
            if (allMessages == null || allMessages.Count == 0)
            {
                txtContext.Text = "No conversation history available.";
                return;
            }

            var recentMessages = allMessages
                .Where(m => IsRelevantMessage(m))
                .OrderBy(m => m.Timestamp)
                .TakeLast(5)
                .ToList();

            StringBuilder context = new StringBuilder();
            foreach (var msg in recentMessages)
            {
                string sender = msg.Sender == currentUsername ? "You" : msg.Sender;
                context.AppendLine($"{sender}: {msg.Message}");
            }

            txtContext.Text = context.ToString();
        }

        private bool IsRelevantMessage(ChatMessage msg)
        {
            if (currentTarget.StartsWith("Group"))
            {
                return msg.Receiver == currentTarget;
            }
            else
            {
                bool isOutgoing = msg.Sender == currentUsername && msg.Receiver == currentTarget;
                bool isIncoming = msg.Sender == currentTarget && msg.Receiver == currentUsername;
                return isOutgoing || isIncoming;
            }
        }

        private async void BtnGenerateReply_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtContext.Text))
            {
                MessageBox.Show("Please provide conversation context!", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnGenerateReply.Enabled = false;
            btnGenerateReply.Text = "⏳ Generating...";
            txtReplyResult.Text = "Please wait...";

            try
            {
                string context = txtContext.Text;
                string tone = cmbTone.SelectedItem.ToString();

                string reply = await GenerateSmartReply(context, tone);
                txtReplyResult.Text = reply;
                txtReplyResult.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                txtReplyResult.Text = $"Error: {ex.Message}";
                txtReplyResult.ForeColor = Color.Red;
            }
            finally
            {
                btnGenerateReply.Enabled = true;
                btnGenerateReply.Text = "✨ Generate Smart Reply";
            }
        }

        private async Task<string> GenerateSmartReply(string context, string tone)
        {
            await Task.Delay(1500);

            var replies = new Dictionary<string, string[]>
            {
                ["Friendly"] = new[] {
                    "That sounds great! Let me know if you need any help with that 😊",
                    "Thanks for sharing! I appreciate your thoughts on this.",
                    "Awesome! Looking forward to it!"
                },
                ["Professional"] = new[] {
                    "Thank you for the information. I will review this and get back to you.",
                    "I appreciate your prompt response. Let's proceed accordingly.",
                    "Understood. I'll make the necessary arrangements."
                },
                ["Casual"] = new[] {
                    "Cool, sounds good to me!",
                    "Yeah, I'm down for that!",
                    "Nice! Let's do it."
                },
                ["Formal"] = new[] {
                    "I acknowledge receipt of your message and will respond accordingly.",
                    "Thank you for bringing this to my attention.",
                    "I appreciate your detailed explanation."
                },
                ["Humorous"] = new[] {
                    "Ha! That's hilarious 😂 I'm totally in!",
                    "You know what? That actually makes perfect sense... or does it? 🤔",
                    "I see what you did there! 😄"
                }
            };

            if (replies.ContainsKey(tone))
            {
                Random rand = new Random();
                return replies[tone][rand.Next(replies[tone].Length)];
            }

            return "Thank you for your message. I'll get back to you soon!";
        }

        #endregion

        #region Translation Tab

        private void BuildTranslationTab()
        {
            tabTranslation = new TabPage("🌍 Translation");

            Label lblInstruction = new Label
            {
                Text = "Translate messages instantly using the Bot's logic:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            Label lblInput = new Label
            {
                Text = "Text to translate:",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtTranslateInput = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(620, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9)
            };

            Label lblFrom = new Label
            {
                Text = "From:",
                Location = new Point(20, 185),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            cmbSourceLang = new ComboBox
            {
                Location = new Point(80, 182),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSourceLang.Items.AddRange(new object[] {
                "auto", "en", "vi", "zh-CN", "ja", "ko", "fr", "es", "de", "ru", "th", "id"
            });
            cmbSourceLang.SelectedIndex = 0;

            Label lblTo = new Label
            {
                Text = "To:",
                Location = new Point(260, 185),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            cmbTargetLang = new ComboBox
            {
                Location = new Point(300, 182),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbTargetLang.Items.AddRange(new object[] {
                "en", "vi", "zh-CN", "ja", "ko", "fr", "es", "de", "ru", "th", "id"
            });
            cmbTargetLang.SelectedIndex = 0;

            btnTranslate = new Button
            {
                Text = "🔄 Translate",
                Location = new Point(480, 177),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(0, 132, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTranslate.FlatAppearance.BorderSize = 0;
            btnTranslate.Click += BtnTranslate_Click;

            Label lblResult = new Label
            {
                Text = "Translation:",
                Location = new Point(20, 230),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtTranslateResult = new RichTextBox
            {
                Location = new Point(20, 255),
                Size = new Size(620, 120),
                ReadOnly = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Button btnCopyTranslation = new Button
            {
                Text = "📋 Copy Translation",
                Location = new Point(20, 390),
                Size = new Size(160, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCopyTranslation.FlatAppearance.BorderSize = 0;
            btnCopyTranslation.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtTranslateResult.Text))
                {
                    Clipboard.SetText(txtTranslateResult.Text);
                    MessageBox.Show("Translation copied to clipboard!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            tabTranslation.Controls.AddRange(new Control[]
            {
                lblInstruction, lblInput, txtTranslateInput,
                lblFrom, cmbSourceLang, lblTo, cmbTargetLang,
                btnTranslate, lblResult, txtTranslateResult, btnCopyTranslation
            });

            tabControl.TabPages.Add(tabTranslation);
        }

        private async void BtnTranslate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTranslateInput.Text))
            {
                MessageBox.Show("Please enter text to translate!", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnTranslate.Enabled = false;
            btnTranslate.Text = "⏳ Translating...";
            txtTranslateResult.Text = "Please wait...";

            try
            {
                string source = cmbSourceLang.SelectedItem.ToString();
                string target = cmbTargetLang.SelectedItem.ToString();
                string text = txtTranslateInput.Text;

                string translation = await TranslateTextGoogle(text, source, target);
                txtTranslateResult.Text = translation;
                txtTranslateResult.ForeColor = Color.Black;
                TranslatedText = translation;
            }
            catch (Exception ex)
            {
                txtTranslateResult.Text = $"Translation error: {ex.Message}\n\nPlease check your internet connection.";
                txtTranslateResult.ForeColor = Color.Red;
            }
            finally
            {
                btnTranslate.Enabled = true;
                btnTranslate.Text = "🔄 Translate";
            }
        }

        private async Task<string> TranslateTextGoogle(string text, string sourceLang, string targetLang)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={sourceLang}&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";
                    
                    var response = await client.GetStringAsync(url);
                    
                    using (JsonDocument doc = JsonDocument.Parse(response))
                    {
                        var root = doc.RootElement;
                        StringBuilder result = new StringBuilder();
                        
                        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                        {
                            var firstElement = root[0];
                            if (firstElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in firstElement.EnumerateArray())
                                {
                                    if (item.ValueKind == JsonValueKind.Array && item.GetArrayLength() > 0)
                                    {
                                        result.Append(item[0].GetString());
                                    }
                                }
                            }
                        }
                        
                        return result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Translation failed: {ex.Message}");
            }
        }

        #endregion

        #region Search Tab

        private void BuildSearchTab()
        {
            tabSearch = new TabPage("🔍 Search");

            Label lblInstruction = new Label
            {
                Text = "Search through conversation history:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            Label lblQuery = new Label
            {
                Text = "Search query:",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtSearchQuery = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(500, 25),
                Font = new Font("Segoe UI", 10)
            };
            txtSearchQuery.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    BtnSearch_Click(s, e);
                }
            };

            btnSearch = new Button
            {
                Text = "🔍 Search",
                Location = new Point(530, 72),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 132, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += BtnSearch_Click;

            lblSearchCount = new Label
            {
                Text = "Results: 0",
                Location = new Point(20, 115),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Gray
            };

            lstSearchResults = new ListBox
            {
                Location = new Point(20, 145),
                Size = new Size(620, 280),
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle,
                DrawMode = DrawMode.OwnerDrawVariable
            };
            lstSearchResults.MeasureItem += LstSearchResults_MeasureItem;
            lstSearchResults.DrawItem += LstSearchResults_DrawItem;
            lstSearchResults.DoubleClick += (s, e) =>
            {
                if (lstSearchResults.SelectedItem != null)
                {
                    var result = (SearchResult)lstSearchResults.SelectedItem;
                    MessageBox.Show($"From: {result.Sender}\nTime: {result.Timestamp:yyyy-MM-dd HH:mm}\n\n{result.Message}",
                        "Message Detail", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            tabSearch.Controls.AddRange(new Control[]
            {
                lblInstruction, lblQuery, txtSearchQuery,
                btnSearch, lblSearchCount, lstSearchResults
            });

            tabControl.TabPages.Add(tabSearch);
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            string query = txtSearchQuery.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter a search query!", "Input Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lstSearchResults.Items.Clear();

            var results = allMessages
                .Where(m => IsRelevantMessage(m))
                .Where(m => m.Message.ToLower().Contains(query) ||
                           m.Sender.ToLower().Contains(query))
                .OrderByDescending(m => m.Timestamp)
                .Select(m => new SearchResult
                {
                    Sender = m.Sender,
                    Message = m.Message,
                    Timestamp = m.Timestamp
                })
                .ToList();

            foreach (var result in results)
            {
                lstSearchResults.Items.Add(result);
            }

            lblSearchCount.Text = $"Results: {results.Count}";
            lblSearchCount.ForeColor = results.Count > 0 ? Color.Green : Color.Red;

            if (results.Count == 0)
            {
                MessageBox.Show($"No messages found containing '{query}'", "No Results",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LstSearchResults_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstSearchResults.Items.Count) return;
            e.ItemHeight = 60;
        }

        private void LstSearchResults_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lstSearchResults.Items.Count) return;

            e.DrawBackground();
            var result = (SearchResult)lstSearchResults.Items[e.Index];

            Color bgColor = e.Index % 2 == 0 ? Color.White : Color.FromArgb(245, 245, 245);
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                bgColor = Color.FromArgb(200, 220, 255);

            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string header = $"{result.Sender} - {result.Timestamp:HH:mm, MMM dd}";
            using (var headerBrush = new SolidBrush(Color.FromArgb(0, 132, 255)))
            using (var headerFont = new Font("Segoe UI", 8, FontStyle.Bold))
            {
                e.Graphics.DrawString(header, headerFont, headerBrush,
                    new PointF(e.Bounds.Left + 5, e.Bounds.Top + 5));
            }

            string preview = result.Message.Length > 80
                ? result.Message.Substring(0, 77) + "..."
                : result.Message;

            using (var msgBrush = new SolidBrush(Color.Black))
            using (var msgFont = new Font("Segoe UI", 9))
            {
                e.Graphics.DrawString(preview, msgFont, msgBrush,
                    new RectangleF(e.Bounds.Left + 5, e.Bounds.Top + 25, e.Bounds.Width - 10, 30));
            }

            e.DrawFocusRectangle();
        }

        #endregion

        #region Summarization Tab

        private void BuildSummarizationTab()
        {
            tabSummarize = new TabPage("📝 Summarize");

            Label lblInstruction = new Label
            {
                Text = "Generate smart summary of conversation:",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };

            Label lblLength = new Label
            {
                Text = "Summary Length:",
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            cmbSummaryLength = new ComboBox
            {
                Location = new Point(150, 47),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSummaryLength.Items.AddRange(new object[] { "Brief", "Medium", "Detailed" });
            cmbSummaryLength.SelectedIndex = 1;

            btnLoadConversation = new Button
            {
                Text = "📥 Load Conversation",
                Location = new Point(320, 43),
                Size = new Size(170, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLoadConversation.FlatAppearance.BorderSize = 0;
            btnLoadConversation.Click += BtnLoadConversation_Click;

            btnGenerateSummary = new Button
            {
                Text = "✨ Generate Summary",
                Location = new Point(500, 43),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(0, 132, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnGenerateSummary.FlatAppearance.BorderSize = 0;
            btnGenerateSummary.Click += BtnGenerateSummary_Click;

            Label lblResult = new Label
            {
                Text = "Summary:",
                Location = new Point(20, 95),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            txtSummaryResult = new RichTextBox
            {
                Location = new Point(20, 120),
                Size = new Size(620, 280),
                ReadOnly = true,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            Button btnCopySummary = new Button
            {
                Text = "📋 Copy Summary",
                Location = new Point(20, 410),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCopySummary.FlatAppearance.BorderSize = 0;
            btnCopySummary.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtSummaryResult.Text))
                {
                    Clipboard.SetText(txtSummaryResult.Text);
                    MessageBox.Show("Summary copied to clipboard!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            tabSummarize.Controls.AddRange(new Control[]
            {
                lblInstruction, lblLength, cmbSummaryLength,
                btnLoadConversation, btnGenerateSummary,
                lblResult, txtSummaryResult, btnCopySummary
            });

            tabControl.TabPages.Add(tabSummarize);
        }

        private void BtnLoadConversation_Click(object sender, EventArgs e)
        {
            if (allMessages == null || allMessages.Count == 0)
            {
                MessageBox.Show("No conversation history available!", "No Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var relevantMessages = allMessages
                .Where(m => IsRelevantMessage(m))
                .OrderBy(m => m.Timestamp)
                .ToList();

            if (relevantMessages.Count == 0)
            {
                MessageBox.Show("No messages found for current conversation!", "No Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtSummaryResult.Text = $"Loaded {relevantMessages.Count} messages from conversation.\n\n" +
                                   $"Date range: {relevantMessages.First().Timestamp:MMM dd} - {relevantMessages.Last().Timestamp:MMM dd}\n" +
                                   $"Participants: {string.Join(", ", relevantMessages.Select(m => m.Sender).Distinct())}\n\n" +
                                   "Click 'Generate Summary' to create BOT summary.";

            btnGenerateSummary.Enabled = true;

            MessageBox.Show($"Loaded {relevantMessages.Count} messages!", "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void BtnGenerateSummary_Click(object sender, EventArgs e)
        {
            btnGenerateSummary.Enabled = false;
            btnGenerateSummary.Text = "⏳ Generating...";
            txtSummaryResult.Text = "Analyzing conversation and generating summary...\nThis may take a moment.";

            try
            {
                var relevantMessages = allMessages
                    .Where(m => IsRelevantMessage(m))
                    .OrderBy(m => m.Timestamp)
                    .ToList();

                string length = cmbSummaryLength.SelectedItem.ToString();
                string summary = await GenerateSummary(relevantMessages, length);

                txtSummaryResult.Text = summary;
                txtSummaryResult.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                txtSummaryResult.Text = $"Error generating summary: {ex.Message}";
                txtSummaryResult.ForeColor = Color.Red;
            }
            finally
            {
                btnGenerateSummary.Enabled = true;
                btnGenerateSummary.Text = "✨ Generate Summary";
            }
        }

        private async Task<string> GenerateSummary(List<ChatMessage> messages, string length)
        {
            await Task.Delay(2000);

            if (messages.Count == 0)
                return "No messages to summarize.";

            var participants = messages.Select(m => m.Sender).Distinct().ToList();
            var messagesByUser = messages.GroupBy(m => m.Sender)
                .ToDictionary(g => g.Key, g => g.Count());

            DateTime start = messages.First().Timestamp;
            DateTime end = messages.Last().Timestamp;
            TimeSpan duration = end - start;

            StringBuilder summary = new StringBuilder();
            summary.AppendLine("📊 CONVERSATION SUMMARY");
            summary.AppendLine("=" + new string('=', 50));
            summary.AppendLine();

            summary.AppendLine($"📅 Period: {start:MMM dd, yyyy} - {end:MMM dd, yyyy}");
            summary.AppendLine($"⏱️ Duration: {duration.Days} days, {duration.Hours} hours");
            summary.AppendLine($"💬 Total Messages: {messages.Count}");
            summary.AppendLine($"👥 Participants: {string.Join(", ", participants)}");
            summary.AppendLine();

            summary.AppendLine("📈 Message Distribution:");
            foreach (var kvp in messagesByUser.OrderByDescending(x => x.Value))
            {
                double percentage = (kvp.Value * 100.0) / messages.Count;
                summary.AppendLine($"  • {kvp.Key}: {kvp.Value} messages ({percentage:F1}%)");
            }
            summary.AppendLine();

            if (length == "Medium" || length == "Detailed")
            {
                summary.AppendLine("🔑 Key Topics Discussed:");
                var topicKeywords = ExtractKeywords(messages);
                foreach (var keyword in topicKeywords.Take(5))
                {
                    summary.AppendLine($"  • {keyword}");
                }
                summary.AppendLine();
            }

            if (length == "Detailed")
            {
                summary.AppendLine("📝 Recent Messages:");
                foreach (var msg in messages.TakeLast(5))
                {
                    string preview = msg.Message.Length > 50
                        ? msg.Message.Substring(0, 47) + "..."
                        : msg.Message;
                    summary.AppendLine($"  • [{msg.Timestamp:HH:mm}] {msg.Sender}: {preview}");
                }
                summary.AppendLine();
            }

            summary.AppendLine("✅ Summary generated successfully!");
            summary.AppendLine("\n(Note: In production, we use BOT for deeper analysis)");

            return summary.ToString();
        }

        private List<string> ExtractKeywords(List<ChatMessage> messages)
        {
            var words = messages
                .SelectMany(m => m.Message.Split(new[] { ' ', ',', '.', '!', '?' },
                    StringSplitOptions.RemoveEmptyEntries))
                .Where(w => w.Length > 4)
                .GroupBy(w => w.ToLower())
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(10)
                .ToList();

            return words;
        }

        #endregion

        #region Helper Classes

        private class SearchResult
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public class ChatMessage
        {
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public string Message { get; set; }
            public string ContentType { get; set; }
            public DateTime Timestamp { get; set; }
            public string MessageID { get; set; }
            public string Status { get; set; }
        }

        #endregion
    }
}