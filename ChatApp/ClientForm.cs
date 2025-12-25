using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ChatApp
{
    public partial class ClientForm : Form
    {
        private ChatClient client;
        private FlowLayoutPanel chatPanel;
        private TextBox messageBox;
        private Button sendBtn;
        private Button connectBtn;
        private TextBox hostInput;
        private NumericUpDown portInput;
        private TextBox usernameInput;
        private ComboBox userListBox;
        private Label statusLabel;
        private Button attachImageBtn;
        private Button attachAudioBtn;
        private Button btnVideoCall;
        private Button btnGames;
        private Button btnAI;


        private System.Windows.Forms.Timer typingTimer;
        private bool isCurrentlyTyping = false;
        private Label lblTypingNotify;


        // đống này quản lí group, động vô = ko chạy dc đừng có khóc nha :DD
        private bool isUpdatingUI = false;
        private Button btnCreateGroup;
        private Button btnLeaveGroup; // nút leavegroup
        private ListBox groupListBox;
        private int currentGroupId = 0;
        private Dictionary<int, GroupInfo> availableGroups = new Dictionary<int, GroupInfo>();
        private Dictionary<string, List<ChatMessage>> temporaryMessages;

        // biến quản lí file & trạng thái
        private static object fileLock = new object(); // khóa file lại 
        private Dictionary<string, byte[]> audioFiles = new Dictionary<string, byte[]>();
        private int audioCounter = 0;
        private string loggedInUsername = "";

        // Dictionary quản lí UI cho biết trạng thái của msg
        private Dictionary<string, Label> messageStatusLabels = new Dictionary<string, Label>();
        // cái này để gửi seen
        private Dictionary<string, string> lastMsgMap = new Dictionary<string, string>();
        // cái này để xử mấy thằng tới sớm
        private Dictionary<string, string> pendingStatusUpdates = new Dictionary<string, string>();

        private Dictionary<string, string> lastSenderMap = new Dictionary<string, string>();
        private Dictionary<string, HashSet<string>> typingMap = new Dictionary<string, HashSet<string>>();

        public string CurrentUsername => string.IsNullOrEmpty(loggedInUsername) ? usernameInput?.Text : loggedInUsername;
        public ClientForm()
        {
            InitializeComponent();
            BuildUI();
            SetupForm();
            typingTimer = new System.Windows.Forms.Timer();
            typingTimer.Interval = 2000; // 2s kh gõ = cút
            typingTimer.Tick += (s, e) =>
            {
                isCurrentlyTyping = false;
                typingTimer.Stop();
                SendTypingSignal(false); // gửi ngừng gõ lên server
            };
            temporaryMessages = new Dictionary<string, List<ChatMessage>>();
        }

        public ClientForm(string username)
        {
            InitializeComponent();
            loggedInUsername = username;
            BuildUI();
            if (!string.IsNullOrEmpty(username))
            {
                usernameInput.Text = username;
                usernameInput.Enabled = false;
            }
            SetupForm();

            typingTimer = new System.Windows.Forms.Timer();
            typingTimer.Interval = 2000; // 2 giây không gõ thì coi như ngừng
            typingTimer.Tick += (s, e) =>
            {
                isCurrentlyTyping = false;
                typingTimer.Stop();
                SendTypingSignal(false); // gửi tín hiệu ngừng gõ lên Server
            };
            temporaryMessages = new Dictionary<string, List<ChatMessage>>();
        }


        private void SendTypingSignal(bool status)
        {
            if (currentGroupId > 0)
                client.SendTypingStatus("", currentGroupId, status);
            else if (userListBox.SelectedItem != null)
                client.SendTypingStatus(userListBox.SelectedItem.ToString(), 0, status);
        }


        private void SetupForm()
        {
            this.WindowState = FormWindowState.Normal;
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 4. (Tùy chọn) Chặn resize nếu bạn lười fix layout bong bóng
            // this.FormBorderStyle = FormBorderStyle.FixedSingle; 
            // this.MaximizeBox = false; 
            this.AllowDrop = true;
            this.DragEnter += ClientForm_DragEnter;
            this.DragDrop += ClientForm_DragDrop;
        }

        private void BuildUI()
        {
            Text = "E2E Encrypted Chat";
            Size = new Size(1400, 900);
            BackColor = Color.White;

            // này thanh trên cùng
            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.White,
                Padding = new Padding(15, 10, 15, 10)
            };

            var hostLabel = new Label { Text = "Host:", Location = new Point(15, 18), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            hostInput = new TextBox { Location = new Point(65, 15), Width = 100, Text = "127.0.0.1", Font = new Font("Segoe UI", 9) };

            var portLabel = new Label { Text = "Port:", Location = new Point(175, 18), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            portInput = new NumericUpDown { Location = new Point(220, 15), Width = 70, Minimum = 1000, Maximum = 65535, Value = 5000, Enabled = false, Font = new Font("Segoe UI", 9) };

            var nameLabel = new Label { Text = "Name:", Location = new Point(300, 18), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            usernameInput = new TextBox { Location = new Point(355, 15), Width = 120, Font = new Font("Segoe UI", 9) };

            connectBtn = new Button { Text = "Connect", Location = new Point(490, 13), Width = 100, Height = 32, BackColor = Color.FromArgb(0, 132, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            connectBtn.FlatAppearance.BorderSize = 0;

            btnVideoCall = new Button { Text = "📹 Video Call", Location = new Point(600, 13), Width = 120, Height = 32, BackColor = Color.Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand };
            btnVideoCall.FlatAppearance.BorderSize = 0;
            btnVideoCall.Click += btnVideoCall_Click;

            statusLabel = new Label { Text = "● Not connected", Location = new Point(15, 45), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

            btnAI = new Button
            {
                Text = "🤖 BOT",
                Location = new Point(730, 13),  // MỚI: 730 - GẦN HƠN
                Width = 80,
                Height = 32,
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            btnAI.FlatAppearance.BorderSize = 0;
            btnAI.Click += btnAI_Click;

            topBar.Controls.AddRange(new Control[]
            {
                hostLabel, hostInput, portLabel, portInput, nameLabel, usernameInput,
                connectBtn, btnVideoCall, btnAI, statusLabel  // MỚI: Bỏ btnGames
            });

            // này thanh ở side 
            Panel sidebar = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Color.FromArgb(245, 246, 247) };
            Label sidebarTitle = new Label { Text = "CONVERSATIONS", Location = new Point(15, 15), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(101, 103, 107) };
            var userLabel = new Label { Text = "Send to:", Location = new Point(15, 50), AutoSize = true, Font = new Font("Segoe UI", 9), ForeColor = Color.FromArgb(101, 103, 107) };

            userListBox = new ComboBox
            {
                Location = new Point(15, 75),
                Width = 220,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false,
                Font = new Font("Segoe UI", 10),
                FlatStyle = FlatStyle.Flat,
                DrawMode = DrawMode.OwnerDrawFixed, 
                ItemHeight = 40 
            };

            userListBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;

                e.DrawBackground();

                string text = userListBox.Items[e.Index].ToString();
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit; // thêm cái này cho chữ nét hơn

                Rectangle rect = e.Bounds;
                int size = rect.Height - 10;
                int x = rect.X + 5;
                int y = rect.Y + 5;

                // vẽ hình tròn nền
                Color bg = GetAvatarColor(text);
                using (Brush b = new SolidBrush(bg))
                {
                    g.FillEllipse(b, x, y, size, size);
                }

                // vẽ Icon hoặc Ký tự đầu
                string letter = "U";
                // mặc định dùng font thường
                string fontName = "Segoe UI";

                if (text == "ALL")
                {
                    letter = "📢";
                    // phải có cái này mới ra loa :))
                    fontName = "Segoe UI Emoji";
                }
                else if (text.Length > 0)
                {
                    letter = text.Substring(0, 1).ToUpper();
                }

               
                using (Font f = new Font(fontName, 12, FontStyle.Bold))
                using (Brush b = new SolidBrush(Color.White))
                {
                    
                    SizeF stringSize = g.MeasureString(letter, f);
                    float textX = x + (size - stringSize.Width) / 2;
                    float textY = y + (size - stringSize.Height) / 2;
                    g.DrawString(letter, f, b, textX, textY);
                }

                
                using (Font f = new Font("Segoe UI", 10, FontStyle.Regular))
                using (Brush b = new SolidBrush(e.ForeColor))
                {
                    g.DrawString(text, f, b, x + size + 10, y + (size - 20) / 2);
                }

                e.DrawFocusRectangle();
            };

            // xử lí userlist:
            userListBox.SelectedIndexChanged += (s, e) =>
            {
                if (isUpdatingUI) return;
                if (userListBox.SelectedItem == null) return;

                string target = userListBox.SelectedItem.ToString();

                isUpdatingUI = true;
                try
                {
                    // nếu chọn user cụ thể thì sao:
                    if (target != "ALL")
                    {
                        currentGroupId = 0;
                        btnLeaveGroup.Visible = false;

                        // bỏ chọn group để tránh lộn status nữa
                        groupListBox.ClearSelected();
                    }
                    else
                    {
                        // ngược lại với bên trên
                        if (currentGroupId == 0)
                            btnLeaveGroup.Visible = false;
                    }
                }
                finally
                {
                    isUpdatingUI = false;
                }

                // load chat thôi
                chatPanel.Controls.Clear();
                LoadChatHistory(target);

                // gửi seen cho tin cuối luôn, này tin nhắn riêng mới dc thôi ku, chứ tin nhắn nhóm sus quá anh ko làm :^((
                if (target != "ALL" && lastMsgMap.ContainsKey(target))
                {
                    client.SendStatusUpdate(
                        target,
                        lastMsgMap[target],
                        "SEEN"
                    );
                }
            };


            var separator = new Panel { Location = new Point(15, 120), Size = new Size(220, 1), BackColor = Color.FromArgb(200, 200, 200) };
            var groupLabel = new Label { Text = "GROUPS", Location = new Point(15, 135), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(101, 103, 107) };

            btnCreateGroup = new Button { Text = "+ Create Group", Location = new Point(15, 160), Size = new Size(220, 30), BackColor = Color.FromArgb(0, 150, 136), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, Enabled = false };
            btnCreateGroup.FlatAppearance.BorderSize = 0;
            btnCreateGroup.Click += (s, e) => CreateGroup();


            groupListBox = new ListBox
            {
                Location = new Point(15, 200),
                Size = new Size(220, 250),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                Enabled = false,
                DrawMode = DrawMode.OwnerDrawFixed, 
                ItemHeight = 45 
            };

            groupListBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0 || e.Index >= groupListBox.Items.Count) return;

                e.DrawBackground();

                
                GroupInfo group = (GroupInfo)groupListBox.Items[e.Index];
                string text = group.GroupName;

                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                Rectangle rect = e.Bounds;
                int size = rect.Height - 10;
                int x = rect.X + 5;
                int y = rect.Y + 5;

                
                Color bg = GetAvatarColor(text);
                using (Brush b = new SolidBrush(bg))
                {
                    g.FillEllipse(b, x, y, size, size);
                }

                
                string letter = text.Length > 0 ? text.Substring(0, 1).ToUpper() : "#";
                using (Font f = new Font("Segoe UI", 12, FontStyle.Bold))
                using (Brush b = new SolidBrush(Color.White))
                {
                    SizeF stringSize = g.MeasureString(letter, f);
                    g.DrawString(letter, f, b, x + (size - stringSize.Width) / 2, y + (size - stringSize.Height) / 2);
                }

                
                using (Font fName = new Font("Segoe UI", 10, FontStyle.Bold))
                using (Brush b = new SolidBrush(e.ForeColor))
                {
                    g.DrawString(text, fName, b, x + size + 10, y + 2);
                }

                
                using (Font fCount = new Font("Segoe UI", 8, FontStyle.Regular))
                using (Brush b = new SolidBrush(Color.Gray))
                {
                    g.DrawString($"{group.MemberCount} members", fCount, b, x + size + 10, y + 22);
                }

                e.DrawFocusRectangle();
            };

            // nút leave - mặc định thì sẽ để ẩn
            btnLeaveGroup = new Button
            {
                Text = "Leave Group",
                Location = new Point(15, 460),
                Size = new Size(220, 30),
                BackColor = Color.Red,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false // bth thì ẩn
            };
            btnLeaveGroup.FlatAppearance.BorderSize = 0;
            btnLeaveGroup.Click += (s, e) => LeaveCurrentGroup();




            // xử lí logic: group list
            groupListBox.SelectedIndexChanged += (s, e) =>
            {
                if (isUpdatingUI || groupListBox.SelectedItem == null) return;

                var group = (GroupInfo)groupListBox.SelectedItem;
                string groupTarget = $"Group{group.GroupID}";

                isUpdatingUI = true;
                try
                {
                    userListBox.SelectedIndex = -1; // reset ComboBox chat riêng
                    currentGroupId = group.GroupID;
                    btnLeaveGroup.Visible = true;
                }
                finally { isUpdatingUI = false; }

                // gửi seen cho đúng thằng cuối thôi
                if (lastMsgMap.ContainsKey(groupTarget) && lastSenderMap.ContainsKey(groupTarget))
                {
                    string lastSender = lastSenderMap[groupTarget];
                    if (lastSender != CurrentUsername) // không cho tự seen bản thân hen
                    {
                        client.SendStatusUpdate(lastSender, lastMsgMap[groupTarget], "SEEN");
                    }
                }

                JoinGroup(group.GroupID);
            };
            sidebar.Controls.AddRange(new Control[] { sidebarTitle, userLabel, userListBox, separator, groupLabel, btnCreateGroup, groupListBox, btnLeaveGroup });

            // này là chỗ chat thôi
            Panel chatContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            chatPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 0),
                Size = new Size(chatContainer.Width, chatContainer.Height - 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10, 20, 10, 20)
            };
            chatPanel.Resize += ChatPanel_Resize; 

            Panel inputPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.White, Padding = new Padding(15) };

            lblTypingNotify = new Label
            {
                Text = "",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(15, 2)
            };
            inputPanel.Controls.Add(lblTypingNotify);


            Panel messageInputContainer = new Panel
            {
                Location = new Point(15, 20),
                Size = new Size(inputPanel.Width - 220, 40),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(228, 241, 254),
                BorderStyle = BorderStyle.None
            };
            messageInputContainer.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = GetRoundedRectanglePath(messageInputContainer.ClientRectangle, 20))
                    e.Graphics.FillPath(new SolidBrush(Color.FromArgb(228, 241, 254)), path);
            };

            messageBox = new TextBox
            {
                Location = new Point(15, 10),
                Size = new Size(messageInputContainer.Width - 30, messageInputContainer.Height - 20),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Enabled = false,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(228, 241, 254),
                Multiline = true,
                WordWrap = true,
                ScrollBars = ScrollBars.Vertical
            };
            messageInputContainer.Controls.Add(messageBox);
            inputPanel.Controls.Add(messageInputContainer);

            attachImageBtn = new Button { Text = "🖼️", Location = new Point(inputPanel.Width - 181, 20), Size = new Size(50, 40), Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Enabled = false, Font = new Font("Segoe UI", 14), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand };
            attachAudioBtn = new Button { Text = "🎵", Location = new Point(inputPanel.Width - 121, 20), Size = new Size(50, 40), Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Enabled = false, Font = new Font("Segoe UI", 14), FlatStyle = FlatStyle.Flat, BackColor = Color.White, Cursor = Cursors.Hand };
            sendBtn = new Button { Text = "Send", Location = new Point(inputPanel.Width - 64, 20), Size = new Size(60, 40), Anchor = AnchorStyles.Right | AnchorStyles.Bottom, Enabled = false, Font = new Font("Segoe UI", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(0, 132, 255), ForeColor = Color.White, Cursor = Cursors.Hand };

            inputPanel.Controls.AddRange(new Control[] { attachImageBtn, attachAudioBtn, sendBtn });
            chatContainer.Controls.Add(chatPanel);
            chatContainer.Controls.Add(inputPanel);
            Controls.Add(chatContainer);
            Controls.Add(sidebar);
            Controls.Add(topBar);

            connectBtn.Click += (s, e) => Connect();
            sendBtn.Click += (s, e) => SendMessage();
            attachImageBtn.Click += (s, e) => AttachImage();
            attachAudioBtn.Click += (s, e) => AttachAudio();
            messageBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && !e.Shift) { e.SuppressKeyPress = true; SendMessage(); } };

            messageBox.TextChanged += (s, e) => {
                if (!isCurrentlyTyping && messageBox.Text.Length > 0)
                {
                    isCurrentlyTyping = true;
                    SendTypingSignal(true); // này là gửi tín hiệu ngay lúc bắt đầu gõ nè
                    typingTimer.Start();
                }
                else if (isCurrentlyTyping)
                {
                    // nếu mà đang gõ nó ctrl + A rồi delete hết thì coi như ngưng
                    if (messageBox.Text.Length == 0)
                    {
                        isCurrentlyTyping = false;
                        typingTimer.Stop();
                        SendTypingSignal(false);
                    }
                    else
                    {
                        typingTimer.Stop(); // reset lại tgian chờ 2 giây hen
                        typingTimer.Start();
                    }
                }
            };
        }

        private void Connect()
        {
            if (string.IsNullOrWhiteSpace(usernameInput.Text))
            {
                MessageBox.Show("Please enter your name!");
                return;
            }
            try
            {
                client = new ChatClient();

                client.OnTypingStatusReceived += (groupId, sender, isTyping) => {
                    this.Invoke(new Action(() => {
                        if (sender == CurrentUsername) return; // Không tự hiện chính mình

                        string contextKey = groupId > 0 ? $"Group{groupId}" : sender;
                        if (!typingMap.ContainsKey(contextKey)) typingMap[contextKey] = new HashSet<string>();

                        if (isTyping) typingMap[contextKey].Add(sender);
                        else typingMap[contextKey].Remove(sender);

                        bool isCorrectContext = (groupId > 0 && groupId == currentGroupId) ||
                                                (groupId == 0 && userListBox.SelectedItem?.ToString() == sender);

                        if (isCorrectContext && typingMap[contextKey].Count > 0)
                        {
                            string displayNames = string.Join(", ", typingMap[contextKey]);
                            lblTypingNotify.Text = $"{displayNames} {(typingMap[contextKey].Count > 1 ? "are" : "is")} typing...";
                        }
                        else lblTypingNotify.Text = "";
                    }));
                };

                // chỗ này là nhận tin riêng hoặc broadcast
                client.OnMessageReceived += (sender, data, contentType, msgId) =>
                {
                    Invoke(new Action(() =>
                    {
                        if (sender != CurrentUsername)
                        {
                            PlayWindowsSound("Windows Notify Messaging.wav");
                        }

                        if (userListBox.SelectedItem?.ToString() == sender && chatPanel.Controls.Count == 0)
                        {
                            AddTimeSeparator($"Session started at {GetFormattedTime(DateTime.Now)}");
                        }
                        // hiển thị tin nhắn lên màn hình ngay lập tức
                        AddMessageToChat(sender, data, contentType, false, null, msgId, "DELIVERED", DateTime.Now);

                        // lưu tn nhận dc vô file
                        // chỉ lưu tin nhắn riêng (không phải BROADCAST "ALL")
                        if (sender != "ALL")
                        {
                            // lưu dữ liệu xuống file để không bị mất khi logout
                            SaveMessageToFile(sender, CurrentUsername, data, contentType, msgId);
                        }

                        // vẫn lưu vào RAM để hỗ trợ tính năng BOT AI
                        string chatKey = sender == "ALL" ? "ALL" : sender;
                        if (!temporaryMessages.ContainsKey(chatKey))
                            temporaryMessages[chatKey] = new List<ChatMessage>();

                        temporaryMessages[chatKey].Add(new ChatMessage
                        {
                            Sender = sender,
                            Receiver = CurrentUsername,
                            Message = data,
                            ContentType = contentType.ToString(),
                            Timestamp = DateTime.Now,
                            MessageID = msgId,
                            Status = "DELIVERED"
                        });

                        // cập nhật ID tin nhắn cuối để quản lý trạng thái SEEN
                        if (sender != "ALL") lastMsgMap[sender] = msgId;

                        // nếu đang mở đúng cửa sổ chat với người gửi, báo ngay là "Đã xem" (SEEN)
                        if (userListBox.SelectedItem != null &&
                            userListBox.SelectedItem.ToString() == sender &&
                            sender != "ALL")
                        {
                            client.SendStatusUpdate(sender, msgId, "SEEN");
                        }
                    }));
                };

                // cập nhật danh sách user trong đống ComboBox
                client.OnUserListUpdated += (users) =>
                {
                    Invoke(new Action(() =>
                    {
                        var selected = userListBox.SelectedItem?.ToString();
                        userListBox.Items.Clear();
                        userListBox.Items.Add("ALL");
                        foreach (var user in users) userListBox.Items.Add(user);

                        if (selected != null && userListBox.Items.Contains(selected))
                            userListBox.SelectedItem = selected;
                        else
                            userListBox.SelectedIndex = 0;
                    }));
                };

                // chỗ này là cập nhật seen với delivered chưa nè
                client.OnMessageStatusUpdated += (msgId, status) =>
                {
                    this.Invoke(new Action(() =>
                    {
                        // 1. Cập nhật UI
                        if (messageStatusLabels.ContainsKey(msgId))
                        {
                            var label = messageStatusLabels[msgId];
                            if (status == "SEEN")
                            {
                                label.Text = "✔✔";
                                label.ForeColor = Color.FromArgb(0, 132, 255);
                                UpdatePreviousMessagesAsSeen(label.Parent);
                            }
                            else if (status == "DELIVERED" && label.Text != "✔✔")
                            {
                                label.Text = "✔";
                            }
                            if (label.Parent != null) label.Parent.PerformLayout();
                        }
                        else
                        {
                            if (!pendingStatusUpdates.ContainsKey(msgId) || status == "SEEN")
                                pendingStatusUpdates[msgId] = status;
                        }

                        // save nó vô file
                        if (userListBox.SelectedItem != null)
                        {
                            string targetUser = userListBox.SelectedItem.ToString();
                            System.Threading.Tasks.Task.Run(() => UpdateLogStatus(targetUser, msgId, status));
                        }
                    }));
                };

                // chỗ này là chỗ nhận tin nhắn group
                client.OnGroupMessageReceived += (groupId, sender, data, contentType, msgId) =>
                {
                    Invoke(new Action(() =>
                    {
                        string groupTarget = $"Group{groupId}";

                        lastMsgMap[groupTarget] = msgId;
                        lastSenderMap[groupTarget] = sender;

                        if (groupId == currentGroupId)
                        {
                            AddMessageToChat(sender, data, contentType, sender == CurrentUsername, null, msgId, "DELIVERED", DateTime.Now);

                            if (sender != CurrentUsername) client.SendStatusUpdate(sender, msgId, "SEEN");
                        }

                        // LƯU VÀO FILE (CHỈ GROUP)
                        var chatMsg = new ChatMessage
                        {
                            Sender = sender,
                            Receiver = groupTarget,
                            Message = data,
                            ContentType = contentType.ToString(),
                            Timestamp = DateTime.Now,
                            MessageID = msgId,
                            Status = "DELIVERED"
                        };
                        SaveToJsonFile(Path.Combine(Application.StartupPath, "data", "chat_logs", $"{CurrentUsername}.json"), chatMsg);
                    }));
                };


                client.OnGroupListUpdated += (groups) => Invoke(new Action(() => UpdateGroupList(groups)));

                // chỗ này là tạo gr thành công
                client.OnGroupCreated += (groupId, groupName) =>
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Group '{groupName}' created!", "Success");

                        // thêm vào danh sách hiện tại
                        var newGroup = new GroupInfo { GroupID = groupId, GroupName = groupName, MemberCount = 1 };
                        availableGroups[groupId] = newGroup;

                        if (!groupListBox.Items.Contains(newGroup)) groupListBox.Items.Add(newGroup);

                        // cập nhật UI 1 chút
                        if (!groupListBox.Items.Contains(newGroup))
                            groupListBox.Items.Add(newGroup);

                        // lưu danh sách group mới vô file
                        SaveJoinedGroupsList();

                        // chuyển ngay vô group mới
                        currentGroupId = groupId;
                        chatPanel.Controls.Clear();
                        AddMessageToChat("System", $"Welcome to '{groupName}'!", ContentType.Text);

                        // chọn group trong listbox
                        groupListBox.SelectedItem = newGroup;
                    }));
                };

                client.OnVideoCallRequestReceived += (senderName) =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        var result = MessageBox.Show(
                            $"{senderName} wants to video call you?",
                            "Video Call",
                            MessageBoxButtons.YesNo
                        );

                        if (result == DialogResult.Yes)
                        {
                            client.AcceptVideoCall(senderName);

                           
                            LaunchPythonApp(senderName, false);
                        }
                    });
                };

                // bật UI
                btnCreateGroup.Enabled = true;
                groupListBox.Enabled = true;

                int port = 5000;
                client.Connect(hostInput.Text, port, usernameInput.Text);

                statusLabel.Text = $"● Connected as";
                statusLabel.ForeColor = Color.Green;
                Label usernameLabel = new Label { Text = usernameInput.Text, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.Green, Location = new Point(statusLabel.Right, statusLabel.Top) };
                statusLabel.Parent.Controls.Add(usernameLabel);

                connectBtn.Enabled = false;
                hostInput.Enabled = false;
                portInput.Enabled = false;
                usernameInput.Enabled = false;
                messageBox.Enabled = true;
                sendBtn.Enabled = true;
                attachImageBtn.Enabled = true;
                attachAudioBtn.Enabled = true;
                userListBox.Enabled = true;
                btnAI.Enabled = true;

                // load lại group đã tham gia từ cái file ghi bên trên thoy
                LoadJoinedGroupsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}");
            }
        }

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(messageBox.Text)) return;
            if (chatPanel.Controls.Count == 0)
            {
                AddTimeSeparator($"Session started at {GetFormattedTime(DateTime.Now)}");
            }

            sendBtn.Enabled = false;
            string message = messageBox.Text;
            string realMsgId = null;

            try
            {
                if (currentGroupId > 0)
                {
                    // gửi tin group - (Đã có logic lưu file chuẩn)
                    realMsgId = client.SendGroupMessage(currentGroupId, message);
                    string status = "DELIVERED";
                    AddMessageToChat("You", message, ContentType.Text, true, null, realMsgId, status);
                    string target = $"Group{currentGroupId}";
                    SaveMessageToFile(CurrentUsername, target, message, ContentType.Text, realMsgId);
                }
                else
                {
                    // gửi tin riêng
                    if (userListBox.SelectedItem == null)
                    {
                        sendBtn.Enabled = true;
                        return;
                    }
                    string receiver = userListBox.SelectedItem.ToString();

                    if (receiver == "ALL")
                        realMsgId = client.BroadcastMessage(message);
                    else
                        realMsgId = client.SendMessage(receiver, message);

                    // hiển thị lên màn hình
                    AddMessageToChat("You", message, ContentType.Text, true, null, realMsgId, "SENT");

                    // lưu tin nhắn riêng vào file
                    if (receiver != "ALL")
                    {
                        // gọi hàm lưu file để tin nhắn không bị mất khi logout
                        SaveMessageToFile(CurrentUsername, receiver, message, ContentType.Text, realMsgId);
                    }
                    

                    // vẫn giữ lưu vào RAM để phục vụ con BOT AI nếu cần
                    string chatKey = receiver;
                    if (!temporaryMessages.ContainsKey(chatKey))
                        temporaryMessages[chatKey] = new List<ChatMessage>();

                    temporaryMessages[chatKey].Add(new ChatMessage
                    {
                        Sender = CurrentUsername,
                        Receiver = receiver,
                        Message = message,
                        ContentType = "Text",
                        Timestamp = DateTime.Now,
                        MessageID = realMsgId,
                        Status = "SENT"
                    });
                }

                PlayWindowsSound("Windows Navigation Start.wav");
                messageBox.Clear();
                messageBox.Focus();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            finally
            {
                System.Threading.Tasks.Task.Delay(100).ContinueWith(t => Invoke(new Action(() => sendBtn.Enabled = true)));
            }
        }


        // hàm này add tin nhắn vào UI (tính cả status)
        private void AddMessageToChat(string sender, string data, ContentType contentType, bool isMe = false, string fileName = null, string msgId = null, string status = "SENT", DateTime? timestamp = null)
        {
            DateTime time = timestamp ?? DateTime.Now;
            string timeStr = time.ToString("HH:mm");

            string displayId = msgId ?? $"{sender}_{DateTime.Now.Ticks}";
            int availableWidth = chatPanel.ClientSize.Width - 40;

            var messagePanel = new Panel
            {
                Width = availableWidth,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(5, 8, 5, 8),
                Padding = new Padding(0),
                Tag = displayId
            };

            int maxBubbleWidth = Math.Min(550, (int)(availableWidth * 0.65));

            Panel bubble = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MaximumSize = new Size(maxBubbleWidth, 0),
                Padding = new Padding(15, 10, 15, 10),
                BackColor = isMe ? Color.FromArgb(0, 132, 255) : Color.FromArgb(233, 233, 235)
            };

            if (contentType == ContentType.Text)
            {
                bubble.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var path = GetRoundedRectanglePath(bubble.ClientRectangle, 18))
                        e.Graphics.FillPath(new SolidBrush(bubble.BackColor), path);
                };
            }
            else
            {
                bubble.BackColor = Color.Transparent;
                bubble.Padding = new Padding(0);
            }

            int yPos = 0;
            if (!isMe)
            {
                var senderLabel = new Label { Text = sender, AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(101, 103, 107), Location = new Point(0, yPos) };
                bubble.Controls.Add(senderLabel);
                yPos = senderLabel.Bottom + 5;
            }

            switch (contentType)
            {
                case ContentType.Text:
                    var textLabel = new Label { Text = data, AutoSize = true, MaximumSize = new Size(maxBubbleWidth - 30, 0), Location = new Point(0, yPos), Font = new Font("Segoe UI", 10), ForeColor = isMe ? Color.White : Color.Black, Padding = new Padding(0, 0, 0, 5) };
                    bubble.Controls.Add(textLabel);
                    break;

                case ContentType.Image:
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(data);
                        using (var ms = new MemoryStream(imageBytes))
                        {
                            var img = Image.FromStream(ms);
                            var resized = ResizeImage(img, Math.Min(400, maxBubbleWidth - 10), 350);
                            var picBox = new PictureBox { Image = resized, SizeMode = PictureBoxSizeMode.AutoSize, Location = new Point(0, yPos), Cursor = Cursors.Hand, BorderStyle = BorderStyle.None };
                            picBox.Click += (s, e) =>
                            {
                                var viewForm = new Form { Text = "Click to close", Size = new Size(Math.Min(img.Width + 40, 1200), Math.Min(img.Height + 70, 800)), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.SizableToolWindow };
                                var fullPic = new PictureBox { Image = Image.FromStream(new MemoryStream(imageBytes)), SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill, Cursor = Cursors.Hand, BackColor = Color.Black };
                                fullPic.Click += (s2, e2) => viewForm.Close();
                                viewForm.Controls.Add(fullPic);
                                viewForm.ShowDialog();
                            };
                            bubble.Controls.Add(picBox);
                        }
                    }
                    catch { }
                    break;

                case ContentType.Audio:
                    try
                    {
                        byte[] audioBytes = Convert.FromBase64String(data);
                        string audioId = $"audio_{audioCounter++}";
                        audioFiles[audioId] = audioBytes;
                        int audioWidth = Math.Min(380, maxBubbleWidth - 10);
                        var audioContainer = new Panel { Width = audioWidth, Height = 60, Location = new Point(0, yPos), BackColor = isMe ? Color.FromArgb(0, 100, 200) : Color.FromArgb(240, 240, 240), BorderStyle = BorderStyle.FixedSingle };
                        var iconLabel = new Label { Text = "🎵", Font = new Font("Segoe UI", 20), Location = new Point(10, 15), Size = new Size(40, 30), ForeColor = isMe ? Color.White : Color.Black };
                        var fileNameLabel = new Label { Text = fileName ?? "Audio file", Location = new Point(60, 12), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = isMe ? Color.White : Color.Black, MaximumSize = new Size(audioWidth - 150, 20) };
                        var sizeLabel = new Label { Text = FormatFileSize(audioBytes.Length), Location = new Point(60, 32), AutoSize = true, Font = new Font("Segoe UI", 8), ForeColor = isMe ? Color.LightGray : Color.Gray };
                        var playBtn = new Button { Text = "▶", Location = new Point(audioWidth - 85, 15), Size = new Size(35, 30), Tag = audioId, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold), Cursor = Cursors.Hand };
                        playBtn.FlatAppearance.BorderSize = 0;
                        var saveBtn = new Button { Text = "💾", Location = new Point(audioWidth - 45, 15), Size = new Size(35, 30), Tag = audioId, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, Font = new Font("Segoe UI", 10), Cursor = Cursors.Hand };
                        saveBtn.FlatAppearance.BorderSize = 0;
                        playBtn.Click += (s, e) => { var id = ((Button)s).Tag.ToString(); if (audioFiles.ContainsKey(id)) PlayAudio(audioFiles[id], fileName); };
                        saveBtn.Click += (s, e) => { var id = ((Button)s).Tag.ToString(); if (audioFiles.ContainsKey(id)) { using (var sfd = new SaveFileDialog()) { sfd.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.ogg"; sfd.FileName = fileName ?? "audio.mp3"; if (sfd.ShowDialog() == DialogResult.OK) { File.WriteAllBytes(sfd.FileName, audioFiles[id]); MessageBox.Show("Saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); } } } };
                        audioContainer.Controls.AddRange(new Control[] { iconLabel, fileNameLabel, sizeLabel, playBtn, saveBtn });
                        bubble.Controls.Add(audioContainer);
                    }
                    catch { }
                    break;
            }
            bubble.Size = bubble.GetPreferredSize(new Size(maxBubbleWidth, 0));

            Label lblTime = new Label
            {
                Text = timeStr,
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.Gray,
                AutoSize = true
            };

            messagePanel.Controls.Add(bubble);
            messagePanel.Controls.Add(lblTime);

            if (isMe)
            {
                bubble.Location = new Point(availableWidth - bubble.Width - 15, 0);
                lblTime.Location = new Point(bubble.Left - lblTime.Width - 5, bubble.Bottom - lblTime.Height - 5);

                // xử lí hiển thị status
                // chỉ hiện cho tin nhắn Text (hoặc có thể mở rộng cho ảnh/audio nếu m thích thì t làm, nhưng mà bố mệt vkl)
                if (contentType == ContentType.Text)
                {
                    string initialText = "○";
                    Color initialColor = Color.LightGray;

                    if (status == "SEEN")
                    {
                        initialText = "✔✔";
                        initialColor = Color.FromArgb(0, 132, 255);
                    }
                    else if (status == "DELIVERED")
                    {
                        initialText = "✔";
                    }

                    Label lblStatus = new Label
                    {
                        Text = initialText,
                        Font = new Font("Segoe UI", 8),
                        ForeColor = initialColor,
                        AutoSize = false,
                        Size = new Size(50, 15),
                        TextAlign = ContentAlignment.MiddleRight,
                        Location = new Point(availableWidth - 60, bubble.Bottom + 2)
                    };
                    messagePanel.Controls.Add(lblStatus);

                    if (!string.IsNullOrEmpty(msgId))
                    {
                        messageStatusLabels[msgId] = lblStatus;

                        // kiểm tra có gói tin tới sớm kh
                        if (pendingStatusUpdates.ContainsKey(msgId))
                        {
                            string pendingStatus = pendingStatusUpdates[msgId];
                            if (pendingStatus == "SEEN")
                            {
                                lblStatus.Text = "✔✔";
                                lblStatus.ForeColor = Color.FromArgb(0, 132, 255);
                            }
                            else if (pendingStatus == "DELIVERED")
                            {
                                lblStatus.Text = "✔";
                            }
                            pendingStatusUpdates.Remove(msgId);
                        }
                    }
                }
            }
            else
            {
                bubble.Location = new Point(15, 0);
                lblTime.Location = new Point(bubble.Right + 5, bubble.Bottom - lblTime.Height - 5);
            }

            chatPanel.Controls.Add(messagePanel);
            chatPanel.ScrollControlIntoView(messagePanel);
            try { chatPanel.VerticalScroll.Value = chatPanel.VerticalScroll.Maximum; } catch { }
        }

        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();

            int diameter = radius * 2;
            if (rect.Width < diameter) diameter = rect.Width;
            if (rect.Height < diameter) diameter = rect.Height;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void AttachImage()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    
                    SendImageFile(ofd.FileName);
                }
            }
        }

        private void AttachAudio()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Audio Files|*.mp3;*.wav;*.m4a;*.ogg;*.wma";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    
                    SendAudioFile(ofd.FileName);
                }
            }
        }

        private void CreateGroup()
        {
            using (var form = new Form())
            {
                form.Text = "Create New Group";
                form.Size = new Size(350, 150);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                var label = new Label { Text = "Group Name:", Location = new Point(20, 20), AutoSize = true };
                var textBox = new TextBox { Location = new Point(20, 45), Width = 290 };
                var okBtn = new Button { Text = "Create", Location = new Point(155, 80), Size = new Size(75, 30), DialogResult = DialogResult.OK };
                var cancelBtn = new Button { Text = "Cancel", Location = new Point(235, 80), Size = new Size(75, 30), DialogResult = DialogResult.Cancel };
                form.Controls.AddRange(new Control[] { label, textBox, okBtn, cancelBtn });
                form.AcceptButton = okBtn;
                if (form.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    client?.CreateGroup(textBox.Text);
                }
            }
        }

        private void JoinGroup(int groupId)
        {
            client?.JoinGroup(groupId);
            currentGroupId = groupId;
            chatPanel.Controls.Clear();

            // xóa chọn bên UserList
            userListBox.SelectedIndexChanged -= UserListBox_ResetGroup;
            userListBox.SelectedItem = null;
            userListBox.SelectedIndexChanged += UserListBox_ResetGroup;

            // load lịch sử Group
            string groupTarget = $"Group{groupId}";
            LoadChatHistory(groupTarget);

            // lưu lại danh sách group đã join (phòng hờ)
            SaveJoinedGroupsList();

            if (availableGroups.TryGetValue(groupId, out var group))
            {
                AddMessageToChat("System", $"Joined group '{group.GroupName}'", ContentType.Text);
            }
            btnLeaveGroup.Visible = true;
        }

        private void LeaveCurrentGroup()
        {
            if (currentGroupId == 0) return;

            var result = MessageBox.Show("Are you sure you want to leave and DELETE all chat history of this group?",
                                         "Confirm Leave", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                int groupIdToRemove = currentGroupId;
                string groupIdentity = $"Group{groupIdToRemove}";

                // gửi lệnh thoát cho Server
                client.LeaveGroup(groupIdToRemove);

                // xóa tin nhắn của gr này trong json luôn
                DeleteGroupMessages(groupIdentity);

                // cho cút khỏi màn hình thằng này luôn
                if (availableGroups.ContainsKey(groupIdToRemove))
                    availableGroups.Remove(groupIdToRemove);

                groupListBox.Items.Clear();
                foreach (var g in availableGroups.Values) groupListBox.Items.Add(g);

                // lưu lại ds group mới (tất nhiên là kh tính cái kia rồi baby)
                SaveJoinedGroupsList();

                // thông báo + reset màn hình
                currentGroupId = 0;
                chatPanel.Controls.Clear();
                btnLeaveGroup.Visible = false;
                AddMessageToChat("System", "You left the group and data has been deleted.", ContentType.Text);
            }
        }

        // chỗ này là hàm xóa tin nhắn gr
        private void DeleteGroupMessages(string groupReceiverName)
        {
            lock (fileLock)
            {
                try
                {
                    string folder = Path.Combine(Application.StartupPath, "data", "chat_logs");
                    string filePath = Path.Combine(folder, $"{CurrentUsername}.json");

                    if (File.Exists(filePath))
                    {
                        string json = File.ReadAllText(filePath);
                        var messages = JsonSerializer.Deserialize<List<ChatMessage>>(json);

                        if (messages != null)
                        {
                            // xóa tất cả tin nhắn có người nhận là Group này
                            int removedCount = messages.RemoveAll(m => m.Receiver == groupReceiverName);

                            if (removedCount > 0)
                            {
                                string newJson = JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });
                                File.WriteAllText(filePath, newJson);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting data: " + ex.Message);
                }
            }
        }

        // hàm này để lưu danh sách mấy group đã join
        private void SaveJoinedGroupsList()
        {
            try
            {
                string folder = Path.Combine(Application.StartupPath, "data", "configs");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, $"groups_{CurrentUsername}.json");

                var groups = availableGroups.Values.ToList();
                string json = JsonSerializer.Serialize(groups);
                File.WriteAllText(filePath, json);
            }
            catch { }
        }

        private void PlayWindowsSound(string soundName)
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media", soundName);

                if (File.Exists(path))
                {
                    using (var player = new System.Media.SoundPlayer(path))
                    {
                        player.Play();
                    }
                }
                else
                {
                    // chắc chả ai có máy lỏ ntn đâu nhỉ
                    System.Media.SystemSounds.Beep.Play();
                }
            }
            catch { }
        }

        // hàm này load những cái hàm trên lưu
        private void LoadJoinedGroupsList()
        {
            try
            {
                string folder = Path.Combine(Application.StartupPath, "data", "configs");
                string filePath = Path.Combine(folder, $"groups_{CurrentUsername}.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var groups = JsonSerializer.Deserialize<List<GroupInfo>>(json);

                    if (groups != null)
                    {
                        availableGroups.Clear();
                        groupListBox.Items.Clear();
                        foreach (var group in groups)
                        {
                            availableGroups[group.GroupID] = group;
                            groupListBox.Items.Add(group);
                        }
                    }
                }
            }
            catch { }
        }

        private void UserListBox_ResetGroup(object sender, EventArgs e)
        {
            if (userListBox.SelectedItem != null)
                currentGroupId = 0;
        }

        private void UpdateGroupList(List<GroupInfo> groups)
        {
            if (groupListBox.InvokeRequired)
            {
                return;
            }

            isUpdatingUI = true;
            try
            {
                foreach (var group in groups)
                {

                    availableGroups[group.GroupID] = group;

                    int index = -1;
                    for (int i = 0; i < groupListBox.Items.Count; i++)
                    {
                        if (((GroupInfo)groupListBox.Items[i]).GroupID == group.GroupID)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        groupListBox.Items[index] = group;
                    }
                    else
                    {
                        groupListBox.Items.Add(group);
                    }
                }
            }
            finally
            {
                isUpdatingUI = false;
            }
        }

        private void SaveMessageToFile(string sender, string receiver, string message, ContentType contentType, string msgId)
        {
            try
            {
                string folder = Path.Combine(Application.StartupPath, "data", "chat_logs");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string filePath = Path.Combine(folder, $"{CurrentUsername}.json");

                var chatMessage = new ChatMessage
                {
                    Sender = sender,
                    Receiver = receiver,
                    Message = message,
                    ContentType = contentType.ToString(),
                    Timestamp = DateTime.Now,
                    MessageID = msgId,
                    // gửi vô group thì mặc định là vế đầu, không thì sent
                    Status = receiver.StartsWith("Group") ? "DELIVERED" : "SENT"
                };

                // ghi vô file JSON
                SaveToJsonFile(filePath, chatMessage);
            }
            catch { }
        }


        private void UpdateLogStatus(string targetUser, string msgId, string newStatus)
        {
            try
            {
                string folder = Path.Combine(Application.StartupPath, "data", "chat_logs");
                string filePath = Path.Combine(folder, $"{CurrentUsername}.json");

                lock (fileLock)
                {
                    if (File.Exists(filePath))
                    {
                        string json = File.ReadAllText(filePath);
                        var messages = JsonSerializer.Deserialize<List<ChatMessage>>(json);

                        if (messages != null)
                        {
                            bool changed = false;
                            int targetIndex = messages.FindIndex(m => m.MessageID == msgId);

                            if (targetIndex != -1)
                            {
                                messages[targetIndex].Status = newStatus;
                                changed = true;

                                if (newStatus == "SEEN")
                                {
                                    for (int i = 0; i < targetIndex; i++)
                                    {
                                        if (messages[i].Receiver == targetUser && messages[i].Sender == CurrentUsername && messages[i].Status != "SEEN")
                                        {
                                            messages[i].Status = "SEEN";
                                        }
                                    }
                                }
                            }

                            if (changed)
                            {
                                string newJson = JsonSerializer.Serialize(messages, new JsonSerializerOptions { WriteIndented = true });
                                File.WriteAllText(filePath, newJson);
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveToJsonFile(string filePath, ChatMessage message)
        {
            lock (fileLock)
            {
                try
                {
                    // chỗ này quan trọng, nếu kh có data thì tự tạo cái mới luôn
                    string folder = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    List<ChatMessage> messages = new List<ChatMessage>();

                    if (File.Exists(filePath))
                    {
                        try
                        {
                            string json = File.ReadAllText(filePath);
                            messages = JsonSerializer.Deserialize<List<ChatMessage>>(json)
                                       ?? new List<ChatMessage>();
                        }
                        catch
                        {
                            messages = new List<ChatMessage>();
                        }
                    }

                    var existingMsg = messages.FirstOrDefault(m => m.MessageID == message.MessageID);
                    if (existingMsg == null)
                    {
                        messages.Add(message);
                        string newJson = JsonSerializer.Serialize(
                            messages,
                            new JsonSerializerOptions { WriteIndented = true }
                        );
                        File.WriteAllText(filePath, newJson);
                    }
                }
                catch
                {
                    // nuốt luôn kakakaa
                }
            }
        }

        private void LoadChatHistory(string targetIdentity)
        {
            // xóa UI trước
            chatPanel.Controls.Clear();

            string filePath = Path.Combine(Application.StartupPath, "data", "chat_logs", $"{CurrentUsername}.json");
            if (!File.Exists(filePath))
            {
                AddTimeSeparator($"New session started at {GetFormattedTime(DateTime.Now)}");
                return;
            }

            lock (fileLock)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var allMessages = JsonSerializer.Deserialize<List<ChatMessage>>(json);
                    if (allMessages == null || allMessages.Count == 0)
                    {
                        AddTimeSeparator($"New session started at {GetFormattedTime(DateTime.Now)}");
                        return;
                    }

                    // lọc tn theo đối tượng
                    var filteredMessages = allMessages.Where(m =>
                        // nếu là Group: Lọc theo tên Group
                        (targetIdentity.StartsWith("Group") && m.Receiver == targetIdentity) ||
                        // nếu là Chat riêng: Lọc tin giữa Me => Target hoặc ngược lại :v 
                        (!targetIdentity.StartsWith("Group") && targetIdentity != "ALL" &&
                            ((m.Sender == CurrentUsername && m.Receiver == targetIdentity) ||
                             (m.Sender == targetIdentity && m.Receiver == CurrentUsername))) ||
                        // 
                        (targetIdentity == "ALL" && m.Receiver == "ALL")
                    ).ToList();

                    
                    if (filteredMessages.Count > 0)
                    {
                        // lấy thời gian của tin nhắn đầu tiên có trong lịch sử
                        AddTimeSeparator($"Conversation started at {GetFormattedTime(filteredMessages[0].Timestamp)}");
                    }
                    else
                    {
                        AddTimeSeparator($"New session started at {GetFormattedTime(DateTime.Now)}");
                    }

                    
                    foreach (var msg in filteredMessages)
                    {
                        bool isMe = (msg.Sender == CurrentUsername);
                        ContentType type = Enum.TryParse(msg.ContentType, out ContentType t) ? t : ContentType.Text;
                        AddMessageToChat(msg.Sender, msg.Message, type, isMe, null, msg.MessageID, msg.Status, msg.Timestamp);
                    }
                }
                catch
                {
                    AddTimeSeparator($"Session error at {GetFormattedTime(DateTime.Now)}");
                }
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
            return $"{bytes / (1024 * 1024)} MB";
        }

        private void UpdatePreviousMessagesAsSeen(Control targetPanel)
        {
            foreach (Control panel in chatPanel.Controls)
            {
                if (panel == targetPanel) break;
                if (panel.Tag != null)
                {
                    string oldMsgId = panel.Tag.ToString();
                    if (messageStatusLabels.ContainsKey(oldMsgId))
                    {
                        var oldLabel = messageStatusLabels[oldMsgId];
                        if (oldLabel.Text != "✔✔")
                        {
                            oldLabel.Text = "✔✔";
                            oldLabel.ForeColor = Color.FromArgb(0, 132, 255);
                        }
                    }
                }
            }
        }

        private Image ResizeImage(Image img, int maxW, int maxH)
        {
            if (img.Width <= maxW && img.Height <= maxH) return new Bitmap(img);
            double ratio = Math.Min((double)maxW / img.Width, (double)maxH / img.Height);
            int w = (int)(img.Width * ratio);
            int h = (int)(img.Height * ratio);
            return new Bitmap(img, new Size(w, h));
        }

        private void PlayAudio(byte[] audioData, string fileName)
        {
            try
            {
                string ext = Path.GetExtension(fileName ?? ".mp3");
                string tempFile = Path.Combine(Path.GetTempPath(), $"chat_audio_{Guid.NewGuid()}{ext}");
                File.WriteAllBytes(tempFile, audioData);
                var psi = new ProcessStartInfo { FileName = tempFile, UseShellExecute = true };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot play: {ex.Message}");
            }
        }

        private void LaunchPythonApp(string targetUser, bool isCaller)
        {
            try
            {
                string currentFolder = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(currentFolder, "video_client.py");

                if (!File.Exists(scriptPath)) { MessageBox.Show("Missing video_client.py"); return; }

                string serverIP = "127.0.0.1";
                if (!string.IsNullOrEmpty(hostInput.Text)) serverIP = hostInput.Text;

                string callerFlag = isCaller ? "1" : "0";
                string args = $"\"{scriptPath}\" \"{CurrentUsername}\" \"{targetUser}\" {callerFlag} \"{serverIP}\"";

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "python";
                start.Arguments = args;
                start.UseShellExecute = false;
                start.CreateNoWindow = true;
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.WorkingDirectory = currentFolder;

                Process.Start(start);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Video Error: " + ex.Message);
            }
        }

        private void btnVideoCall_Click(object sender, EventArgs e)
        {
            if (userListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select the person you want to call!");
                return;
            }

            string receiver = userListBox.SelectedItem.ToString();

            if (receiver == "ALL")
            {
                MessageBox.Show("The video call feature only supports one-on-one calls, it cannot be used to call everyone.");
                return;
            }

            if (receiver == this.CurrentUsername)
            {
                MessageBox.Show("You can't call yourself!");
                return;
            }

            if (receiver.StartsWith("Group:") || receiver.StartsWith("#"))
            {
                MessageBox.Show("Cannot make a video call to the group. Please select a specific member in the group to call.");
                return;
            }

            client.RequestVideoCall(receiver);
            LaunchPythonApp(receiver, true);
        }

        private void AddTimeSeparator(string text)
        {
            Label lblTime = new Label
            {
                Text = $"--- {text} ---",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Width = chatPanel.Width - 50,
                Height = 30,
                Margin = new Padding(0, 10, 0, 10)
            };
            chatPanel.Controls.Add(lblTime);
            chatPanel.ScrollControlIntoView(lblTime);
        }


        private string GetFormattedTime(DateTime time)
        {
            string hourMinute = time.ToString("HH:mm");
            if (time.Date == DateTime.Today)
                return $"{hourMinute} Today";
            return $"{hourMinute} {time:dd/MM/yyyy}";
        }


        private void btnAI_Click(object sender, EventArgs e)
        {
            if (client == null || !client.IsConnected)
            {
                MessageBox.Show("Please connect to server first!",
                    "Not Connected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // lấy cái msg hiện tại
                List<AIAssistantDialog.ChatMessage> messages = GetCurrentConversationMessages();

                string target = currentGroupId > 0
                    ? $"Group{currentGroupId}"
                    : (userListBox.SelectedItem?.ToString() ?? "ALL");

                // Open AI
                using (var aiDialog = new AIAssistantDialog(CurrentUsername, target, messages))
                {
                    if (aiDialog.ShowDialog() == DialogResult.OK)
                    {
                        // insert nó vô box luôn
                        if (!string.IsNullOrEmpty(aiDialog.SelectedReply))
                        {
                            messageBox.Text = aiDialog.SelectedReply;
                            messageBox.Focus();
                            messageBox.SelectionStart = messageBox.Text.Length;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"BOT Assistant error: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ClientForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ClientForm_DragDrop(object sender, DragEventArgs e)
        {
            
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string filePath in files)
            {
                
                string ext = Path.GetExtension(filePath).ToLower();

                if (new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" }.Contains(ext))
                {
                    SendImageFile(filePath); 
                }
                else if (new[] { ".mp3", ".wav", ".m4a", ".ogg", ".wma" }.Contains(ext))
                {
                    SendAudioFile(filePath); 
                }
                else
                {
                    MessageBox.Show($"File type '{ext}' not supported yet!", "Warning");
                }
            }
        }

        private void SendImageFile(string filePath)
        {
            try
            {
                byte[] imageData = GetCompressedImageBytes(filePath);
                
                string base64Image = Convert.ToBase64String(imageData);
                string msgId = null;

                if (currentGroupId > 0)
                {
                   
                 
                    msgId = client.SendGroupFile(currentGroupId, imageData, ContentType.Image);

                    
                    AddMessageToChat("You", base64Image, ContentType.Image, true, null, msgId, "DELIVERED");

                    
                    SaveMessageToFile(CurrentUsername, $"Group{currentGroupId}", base64Image, ContentType.Image, msgId);
                }
                else
                {
                    
                    if (userListBox.SelectedItem == null) return;
                    string receiver = userListBox.SelectedItem.ToString();

                    if (receiver == "ALL")
                        msgId = client.BroadcastFile(imageData, ContentType.Image);
                    else
                        msgId = client.SendFile(receiver, imageData, ContentType.Image);

                    
                    AddMessageToChat("You", base64Image, ContentType.Image, true, null, msgId);

                   
                    if (receiver != "ALL")
                    {
                        SaveMessageToFile(CurrentUsername, receiver, base64Image, ContentType.Image, msgId);
                    }

                    
                    string chatKey = receiver;
                    if (!temporaryMessages.ContainsKey(chatKey)) temporaryMessages[chatKey] = new List<ChatMessage>();
                    temporaryMessages[chatKey].Add(new ChatMessage
                    {
                        Sender = CurrentUsername,
                        Receiver = receiver,
                        Message = base64Image, 
                        ContentType = "Image",
                        Timestamp = DateTime.Now,
                        MessageID = msgId,
                        Status = "SENT"
                    });
                }
            }
            catch (Exception ex) { MessageBox.Show("Error sending image: " + ex.Message); }
        }

        private void ChatPanel_Resize(object sender, EventArgs e)
        {
            chatPanel.SuspendLayout(); // tạm ngưng vẽ cho đỡ lag

            int newAvailableWidth = chatPanel.ClientSize.Width - 40;

            foreach (Control ctrl in chatPanel.Controls)
            {
                // bỏ qua mấy th phân cách thời gian
                if (ctrl is Panel messagePanel)
                {
                    // cập nhật chiều rộng của panel chứa tin nhắn
                    messagePanel.Width = newAvailableWidth;

                    // tìm thằng bubble bên trong
                    Panel bubble = messagePanel.Controls.OfType<Panel>().FirstOrDefault();

                    // tìm thằng hiển thị giờ 
                    Label lblTime = messagePanel.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains(":"));
                    Label lblStatus = messagePanel.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "✔" || l.Text == "✔✔" || l.Text == "○");

                    if (bubble != null)
                    {
                        
                        bool isMe = (bubble.BackColor == Color.FromArgb(0, 132, 255));

                        if (isMe)
                        {
                            
                            bubble.Location = new Point(newAvailableWidth - bubble.Width - 15, 0);

                            if (lblTime != null)
                                lblTime.Location = new Point(bubble.Left - lblTime.Width - 5, bubble.Bottom - lblTime.Height - 5);

                            if (lblStatus != null)
                                lblStatus.Location = new Point(newAvailableWidth - 60, bubble.Bottom + 2);
                        }
                        else
                        {
                            
                            bubble.Location = new Point(15, 0);

                            if (lblTime != null)
                                lblTime.Location = new Point(bubble.Right + 5, bubble.Bottom - lblTime.Height - 5);
                        }
                    }
                }
                else if (ctrl is Label separator) 
                {
                    separator.Width = newAvailableWidth;
                }
            }

            chatPanel.ResumeLayout(true); 
        }


        private void SendAudioFile(string filePath)
        {
            try
            {
                byte[] audioData = File.ReadAllBytes(filePath);
                
                string base64Audio = Convert.ToBase64String(audioData);
                string fileName = Path.GetFileName(filePath);
                string msgId = null;

                if (currentGroupId > 0)
                {
                    
                    msgId = client.SendGroupFile(currentGroupId, audioData, ContentType.Audio);
                    AddMessageToChat("You", base64Audio, ContentType.Audio, true, fileName, msgId, "DELIVERED");

                   
                    SaveMessageToFile(CurrentUsername, $"Group{currentGroupId}", base64Audio, ContentType.Audio, msgId);
                }
                else
                {
                    // RIÊNG
                    if (userListBox.SelectedItem == null) return;
                    string receiver = userListBox.SelectedItem.ToString();

                    if (receiver == "ALL")
                        msgId = client.BroadcastFile(audioData, ContentType.Audio);
                    else
                        msgId = client.SendFile(receiver, audioData, ContentType.Audio);

                    AddMessageToChat("You", base64Audio, ContentType.Audio, true, fileName, msgId);

                    
                    if (receiver != "ALL")
                    {
                        SaveMessageToFile(CurrentUsername, receiver, base64Audio, ContentType.Audio, msgId);
                    }

                    // Lưu RAM
                    string chatKey = receiver;
                    if (!temporaryMessages.ContainsKey(chatKey)) temporaryMessages[chatKey] = new List<ChatMessage>();
                    temporaryMessages[chatKey].Add(new ChatMessage
                    {
                        Sender = CurrentUsername,
                        Receiver = receiver,
                        Message = $"[Audio: {fileName}]", 
                        ContentType = "Audio",
                        Timestamp = DateTime.Now,
                        MessageID = msgId,
                        Status = "SENT"
                    });
                 
                }
            }
            catch (Exception ex) { MessageBox.Show("Error sending audio: " + ex.Message); }
        }

        private List<AIAssistantDialog.ChatMessage> GetCurrentConversationMessages()
        {
            List<AIAssistantDialog.ChatMessage> result = new List<AIAssistantDialog.ChatMessage>();

            try
            {
                string target = currentGroupId > 0
                    ? $"Group{currentGroupId}"
                    : (userListBox.SelectedItem?.ToString() ?? "ALL");

                // Nếu là chat riêng, lấy từ temporaryMessages (RAM)
                if (!target.StartsWith("Group"))
                {
                    if (temporaryMessages.ContainsKey(target))
                    {
                        foreach (var msg in temporaryMessages[target])
                        {
                            result.Add(new AIAssistantDialog.ChatMessage
                            {
                                Sender = msg.Sender,
                                Receiver = msg.Receiver,
                                Message = msg.Message,
                                ContentType = msg.ContentType,
                                Timestamp = msg.Timestamp,
                                MessageID = msg.MessageID,
                                Status = msg.Status
                            });
                        }
                    }
                    return result;
                }

                // Nếu là Group, load từ file như bình thường
                string folder = Path.Combine(Application.StartupPath, "data", "chat_logs");
                string filePath = Path.Combine(folder, $"{CurrentUsername}.json");

                if (File.Exists(filePath))
                {
                    string json;
                    lock (fileLock) { json = File.ReadAllText(filePath); }

                    var messages = JsonSerializer.Deserialize<List<ChatMessage>>(json);

                    if (messages != null)
                    {
                        foreach (var msg in messages)
                        {
                            if (msg.Receiver == target)
                            {
                                result.Add(new AIAssistantDialog.ChatMessage
                                {
                                    Sender = msg.Sender,
                                    Receiver = msg.Receiver,
                                    Message = msg.Message,
                                    ContentType = msg.ContentType,
                                    Timestamp = msg.Timestamp,
                                    MessageID = msg.MessageID,
                                    Status = msg.Status
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BOT] Error loading messages: {ex.Message}");
            }

            return result;
        }

        private byte[] GetCompressedImageBytes(string filePath)
        {
            try
            {
                using (var original = Image.FromFile(filePath))
                {
                    
                    int maxWidth = 1280;
                    int maxHeight = 1280;
                    int newWidth = original.Width;
                    int newHeight = original.Height;

                    if (original.Width > maxWidth || original.Height > maxHeight)
                    {
                        double ratioX = (double)maxWidth / original.Width;
                        double ratioY = (double)maxHeight / original.Height;
                        double ratio = Math.Min(ratioX, ratioY);

                        newWidth = (int)(original.Width * ratio);
                        newHeight = (int)(original.Height * ratio);
                    }

                    
                    using (var resized = new Bitmap(newWidth, newHeight))
                    using (var g = Graphics.FromImage(resized))
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        g.DrawImage(original, 0, 0, newWidth, newHeight);

                        
                        using (var ms = new MemoryStream())
                        {
                            
                            var jpegCodec = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders()
                                .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

                            var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                            encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 75L); // Chất lượng 75%

                            resized.Save(ms, jpegCodec, encoderParams);
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch
            {
                // Nếu lỗi nén (ví dụ file ko phải ảnh chuẩn), trả về file gốc ráng chịu
                return File.ReadAllBytes(filePath);
            }
        }

        private Color GetAvatarColor(string name)
        {
            if (name == "ALL") return Color.Orange; 
            int hash = name.GetHashCode();
            Random rnd = new Random(hash);
            
            return Color.FromArgb(rnd.Next(50, 200), rnd.Next(50, 200), rnd.Next(50, 200));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            temporaryMessages.Clear();

            client?.Disconnect();
            base.OnFormClosing(e);
        }

        public class ChatMessage
        {
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public string Message { get; set; }
            public string ContentType { get; set; }
            public DateTime Timestamp { get; set; }

            public string MessageID { get; set; }
            public string Status { get; set; } = "SENT";
        }
    }
}