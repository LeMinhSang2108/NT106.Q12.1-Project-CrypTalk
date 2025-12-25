using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrypTalk
{
    public static class FirebaseHelper
    {
        private const string FIREBASE_URL = "https://cryptalk-9a9c6-default-rtdb.firebaseio.com/";
        private const string FIREBASE_SECRET = "";

        public static FirebaseClient firebaseClient;
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized) return;

            try
            {
                firebaseClient = new FirebaseClient(
                    FIREBASE_URL,
                    new FirebaseOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(FIREBASE_SECRET)
                    }
                );

                isInitialized = true;
                MessageBox.Show("Connected to Firebase Cloud Database!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Firebase connection failed:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        public static async Task<bool> TestConnection()
        {
            try
            {
                if (!isInitialized) Initialize();

                var test = await firebaseClient
                    .Child("test")
                    .OnceSingleAsync<string>();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RegisterUser(string username, string passwordHash, bool isAdmin = false)
        {
            try
            {
                if (!isInitialized) Initialize();
                var existingUser = await firebaseClient
                    .Child("users")
                    .Child(username)
                    .OnceSingleAsync<User>();

                if (existingUser != null)
                {
                    MessageBox.Show("Username already exists!");
                    return false;
                }

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    IsAdmin = isAdmin,
                    CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Email = "",
                    Phone = "",
                    Gender = "",
                    Avatar = ""
                };

                await firebaseClient
                    .Child("users")
                    .Child(username)
                    .PutAsync(newUser);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}");
                return false;
            }
        }

        public static async Task<User> Login(string username, string passwordHash)
        {
            try
            {
                if (!isInitialized) Initialize();

                System.Diagnostics.Debug.WriteLine($"[Firebase] Attempting login for: {username}");

                var user = await firebaseClient
                    .Child("users")
                    .Child(username)
                    .OnceSingleAsync<User>();

                System.Diagnostics.Debug.WriteLine($"[Firebase] User found: {user != null}");

                if (user != null && user.PasswordHash == passwordHash)
                {
                    string newToken = GenerateSessionToken();
                    string deviceId = GetDeviceId();

                    System.Diagnostics.Debug.WriteLine($"[Login] Username: {username}");
                    System.Diagnostics.Debug.WriteLine($"[Login] Generated DeviceId: {deviceId}");
                    System.Diagnostics.Debug.WriteLine($"[Login] Generated Token: {newToken}");

                    if (user.ActiveSession != null && user.ActiveSession.DeviceId == deviceId)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Login] WARNING: Same user, same device - will kick old session!");
                        System.Diagnostics.Debug.WriteLine($"[Login] Old Token: {user.ActiveSession.SessionToken}");
                        System.Diagnostics.Debug.WriteLine($"[Login] New Token: {newToken}");
                    }
                    else if (user.ActiveSession != null && user.ActiveSession.DeviceId != deviceId)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Login] WARNING: Same user, different device - will kick other device!");
                    }

                    var newSession = new SessionInfo
                    {
                        SessionToken = newToken,
                        DeviceId = deviceId,
                        LoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        LastActive = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    await firebaseClient
                        .Child("users")
                        .Child(username)
                        .PatchAsync(new
                        {
                            LastLogin = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            ActiveSession = newSession
                        });

                    SaveSessionToken(username, newToken);
                    System.Diagnostics.Debug.WriteLine($"[Firebase] Login successful for {username}");
                    return user;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firebase] Login exception: {ex}");
                MessageBox.Show($"Login error: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> ValidateSession(string username)
        {
            try
            {
                if (!isInitialized) Initialize();

                var user = await firebaseClient
                    .Child("users")
                    .Child(username)
                    .OnceSingleAsync<User>();

                System.Diagnostics.Debug.WriteLine($"[ValidateSession] Checking for: {username}");
                System.Diagnostics.Debug.WriteLine($"[ValidateSession] User from Firebase: {user != null}");
                System.Diagnostics.Debug.WriteLine($"[ValidateSession] ActiveSession: {user?.ActiveSession != null}");

                if (user?.ActiveSession == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[ValidateSession] No active session on Firebase");
                    return false;
                }

                string localToken = GetSessionToken(username);
                string localDeviceId = GetDeviceId();

                System.Diagnostics.Debug.WriteLine($"[ValidateSession] Local Token: {localToken}");
                System.Diagnostics.Debug.WriteLine($"[ValidateSession] Firebase Token: {user.ActiveSession.SessionToken}");
                System.Diagnostics.Debug.WriteLine($"[ValidateSession] Local Device: {localDeviceId}");
                System.Diagnostics.Debug.WriteLine($"[ValidateSession] Firebase Device: {user.ActiveSession.DeviceId}");

                bool isValid = user.ActiveSession.SessionToken == localToken
                            && user.ActiveSession.DeviceId == localDeviceId;

                System.Diagnostics.Debug.WriteLine($"[ValidateSession] Result: {isValid}");

                return isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ValidateSession] Error: {ex.Message}");
                return false;
            }
        }

        public static async Task<User> GetUser(string username)
        {
            try
            {
                if (!isInitialized) Initialize();

                return await firebaseClient
                    .Child("users")
                    .Child(username)
                    .OnceSingleAsync<User>();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> UpdateProfile(string username, string email, string phone, string gender)
        {
            try
            {
                if (!isInitialized) Initialize();
                var user = await GetUser(username);
                if (user == null) return false;

                user.Email = email.Trim();
                user.Phone = phone.Trim();
                user.Gender = gender;

                await firebaseClient
                    .Child("users")
                    .Child(username)
                    .PutAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Profile update error: {ex.Message}");
                return false;
            }
        }

        public static async Task<bool> ChangePassword(string username, string newPasswordHash)
        {
            try
            {
                if (!isInitialized) Initialize();

                var user = await GetUser(username);
                if (user == null) return false;

                user.PasswordHash = newPasswordHash;
                await firebaseClient
                    .Child("users")
                    .Child(username)
                    .PutAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Change password error: {ex.Message}");
                return false;
            }
        }

        public static async Task<System.Collections.Generic.List<User>> GetAllUsers()
        {
            try
            {
                if (!isInitialized) Initialize();

                var users = await firebaseClient
                    .Child("users")
                    .OnceAsync<User>();

                return users.Select(u => u.Object).ToList();
            }
            catch
            {
                return new System.Collections.Generic.List<User>();
            }
        }

        public static async Task<bool> DeleteUser(string username)
        {
            try
            {
                if (!isInitialized) Initialize();

                await firebaseClient
                    .Child("users")
                    .Child(username)
                    .DeleteAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }


        public static async Task SaveChatMessage(string sender, string receiver, string message, string contentType)
        {
            try
            {
                if (!isInitialized) Initialize();

                var chatMessage = new ChatMessage
                {
                    Sender = sender,
                    Receiver = receiver,
                    Message = message,
                    ContentType = contentType,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                await firebaseClient
                    .Child("chat_logs")
                    .Child(sender)
                    .PostAsync(chatMessage);

                if (receiver != "ALL")
                {
                    await firebaseClient
                        .Child("chat_logs")
                        .Child(receiver)
                        .PostAsync(chatMessage);
                }
            }
            catch { }
        }

        public static async Task<System.Collections.Generic.List<ChatMessage>> GetChatHistory(string username)
        {
            try
            {
                if (!isInitialized) Initialize();

                var messages = await firebaseClient
                    .Child("chat_logs")
                    .Child(username)
                    .OnceAsync<ChatMessage>();

                return messages
                    .Select(m => m.Object)
                    .OrderBy(m => m.Timestamp)
                    .ToList();
            }
            catch
            {
                return new System.Collections.Generic.List<ChatMessage>();
            }
        }

        public static async Task<UserStats> GetUserStats(string username)
        {
            try
            {
                var messages = await GetChatHistory(username);
                var today = DateTime.Now.Date;

                var stats = new UserStats
                {
                    TotalMessages = messages.Count,
                    TodayMessages = messages.Count(m =>
                    {
                        DateTime.TryParse(m.Timestamp, out DateTime dt);
                        return dt.Date == today;
                    }),
                    Friends = messages
                        .Where(m => m.Receiver != "ALL")
                        .Select(m => m.Sender == username ? m.Receiver : m.Sender)
                        .Distinct()
                        .Count()
                };

                return stats;
            }
            catch
            {
                return new UserStats();
            }
        }

        private static string GenerateSessionToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static string GetDeviceId()
        {
            string deviceIdFile = Path.Combine(Application.StartupPath, "device_id.txt");
            string deviceId;

            if (File.Exists(deviceIdFile))
            {
                deviceId = File.ReadAllText(deviceIdFile).Trim();
            }
            else
            {
                deviceId = Guid.NewGuid().ToString("N");
                File.WriteAllText(deviceIdFile, deviceId);
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceId));
                return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 32).ToLower();
            }
        }

        public static void SaveSessionToken(string username, string token)
        {
            string sessionsDir = Path.Combine(Application.StartupPath, "sessions");
            if (!Directory.Exists(sessionsDir))
            {
                Directory.CreateDirectory(sessionsDir);
            }
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            string filePath = Path.Combine(sessionsDir, $"session_{username}_{processId}.txt");
            File.WriteAllText(filePath, token);
            System.Diagnostics.Debug.WriteLine($"[SaveSessionToken] Saved for {username} (PID: {processId})");
        }

        public static string GetSessionToken(string username)
        {
            string sessionsDir = Path.Combine(Application.StartupPath, "sessions");
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            string filePath = Path.Combine(sessionsDir, $"session_{username}_{processId}.txt");

            if (File.Exists(filePath))
            {
                string token = File.ReadAllText(filePath).Trim();
                System.Diagnostics.Debug.WriteLine($"[GetSessionToken] Retrieved for {username} (PID: {processId}): {token}");
                return token;
            }
            System.Diagnostics.Debug.WriteLine($"[GetSessionToken] No token found for {username} (PID: {processId})");
            return "";
        }

        public static void ClearSession(string username)
        {
            string sessionsDir = Path.Combine(Application.StartupPath, "sessions");
            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            string filePath = Path.Combine(sessionsDir, $"session_{username}_{processId}.txt");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                System.Diagnostics.Debug.WriteLine($"[ClearSession] Cleared for {username} (PID: {processId})");
            }
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsAdmin { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public string CreatedAt { get; set; }
        public string LastLogin { get; set; }

        public SessionInfo ActiveSession { get; set; }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Message { get; set; }
        public string ContentType { get; set; }
        public string Timestamp { get; set; }
    }

    public class UserStats
    {
        public int TotalMessages { get; set; }
        public int TodayMessages { get; set; }
        public int Friends { get; set; }
    }

    public class SessionInfo
    {
        public string SessionToken { get; set; }
        public string DeviceId { get; set; }
        public string LoginTime { get; set; }
        public string LastActive { get; set; }
    }
}