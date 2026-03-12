using MongoDB.Driver;
using SeriesTracker.Models;

namespace SeriesTracker.Services
{
    public class AuthService
    {
        private readonly MongoDbService _mongoDb;
        public User? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser != null;

        // Fired whenever login state changes so UI can react
        public event Action? OnAuthStateChanged;

        public AuthService(MongoDbService mongoDb)
        {
            _mongoDb = mongoDb;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(
            string username, string email, string password)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
                return (false, "All fields are required.");

            if (password.Length < 6)
                return (false, "Password must be at least 6 characters.");

            // Check if username or email already exists
            var existingUser = await _mongoDb.Users
                .Find(u => u.Username == username || u.Email == email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                if (existingUser.Username == username)
                    return (false, "Username is already taken.");
                if (existingUser.Email == email)
                    return (false, "Email is already registered.");
            }

            // Hash password and save new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow
            };

            await _mongoDb.Users.InsertOneAsync(user);
            return (true, "Registration successful!");
        }

        public async Task<(bool Success, string Message)> LoginAsync(
            string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
                return (false, "Username and password are required.");

            // Find user by username
            var user = await _mongoDb.Users
                .Find(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
                return (false, "Invalid username or password.");

            // Verify password against stored hash
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (false, "Invalid username or password.");

            CurrentUser = user;
            OnAuthStateChanged?.Invoke();
            return (true, "Login successful!");
        }

        public void Logout()
        {
            CurrentUser = null;
            OnAuthStateChanged?.Invoke();
        }
    }
}