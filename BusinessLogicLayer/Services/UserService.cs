using InfertilityApp.BusinessLogicLayer.Interfaces;
using InfertilityApp.DataAccessLayer.Interfaces;
using InfertilityApp.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace InfertilityApp.BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // CRUD cơ bản
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username);
            return users.FirstOrDefault();
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == email);
            return users.FirstOrDefault();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (!await ValidateUserDataAsync(user))
            {
                throw new ArgumentException("Dữ liệu người dùng không hợp lệ");
            }

            if (!await IsUsernameUniqueAsync(user.Username))
            {
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
            }

            if (!await IsEmailUniqueAsync(user.Email))
            {
                throw new InvalidOperationException("Email đã được sử dụng");
            }

            user.PasswordHash = HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;

            var result = await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            if (!await ValidateUserDataAsync(user))
            {
                throw new ArgumentException("Dữ liệu người dùng không hợp lệ");
            }

            if (!await IsUsernameUniqueAsync(user.Username, user.Id))
            {
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại");
            }

            if (!await IsEmailUniqueAsync(user.Email, user.Id))
            {
                throw new InvalidOperationException("Email đã được sử dụng");
            }

            if (!string.IsNullOrEmpty(user.PasswordHash) && !user.PasswordHash.StartsWith("$2"))
            {
                user.PasswordHash = HashPassword(user.PasswordHash);
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return false;

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Authentication & Authorization
        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive) return null;

            if (VerifyPassword(password, user.PasswordHash))
            {
                await UpdateLastLoginAsync(user.Id);
                return user;
            }

            return null;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Mật khẩu hiện tại không đúng");
            }

            if (!await ValidatePasswordComplexityAsync(newPassword))
            {
                throw new ArgumentException("Mật khẩu mới không đáp ứng yêu cầu bảo mật");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            if (!await ValidatePasswordComplexityAsync(newPassword))
            {
                throw new ArgumentException("Mật khẩu mới không đáp ứng yêu cầu bảo mật");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordForOldHashAsync(string username, string newPassword)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null) return false;

            if (!await ValidatePasswordComplexityAsync(newPassword))
            {
                throw new ArgumentException("Mật khẩu mới không đáp ứng yêu cầu bảo mật");
            }

            user.PasswordHash = HashPassword(newPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserInRoleAsync(int userId, string role)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            return user?.Role?.Equals(role, StringComparison.OrdinalIgnoreCase) == true;
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _unitOfWork.Users.FindAsync(u => u.Role == role && u.IsActive);
        }

        // User management
        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string newRole)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            var validRoles = new[] { "Admin", "Doctor", "Nurse", "Receptionist" };
            if (!validRoles.Contains(newRole))
            {
                throw new ArgumentException("Vai trò không hợp lệ");
            }

            user.Role = newRole;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null) return false;

            user.LastLogin = DateTime.Now;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        // Validation và Security
        public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username);
            if (excludeUserId.HasValue)
            {
                users = users.Where(u => u.Id != excludeUserId.Value);
            }
            return !users.Any();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == email);
            if (excludeUserId.HasValue)
            {
                users = users.Where(u => u.Id != excludeUserId.Value);
            }
            return !users.Any();
        }

        public async Task<bool> ValidatePasswordComplexityAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]");

            return await Task.FromResult(hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar);
        }

        public async Task<bool> ValidateUserDataAsync(User user)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(user.Username) ||
                string.IsNullOrWhiteSpace(user.FullName) ||
                string.IsNullOrWhiteSpace(user.Role))
                return false;

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(user.Email))
                    return false;
            }

            // Validate role
            var validRoles = new[] { "Admin", "Doctor", "Nurse", "Receptionist" };
            if (!validRoles.Contains(user.Role))
                return false;

            return await Task.FromResult(true);
        }

        // Statistics
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _unitOfWork.Users.CountAsync(u => u.IsActive);
        }

        public async Task<Dictionary<string, int>> GetUsersByRoleStatisticsAsync()
        {
            var activeUsers = await _unitOfWork.Users.FindAsync(u => u.IsActive);
            return activeUsers.GroupBy(u => u.Role ?? "Không xác định")
                             .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetActiveUsersStatisticsAsync()
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var stats = new Dictionary<string, int>
            {
                ["Hoạt động"] = allUsers.Count(u => u.IsActive),
                ["Không hoạt động"] = allUsers.Count(u => !u.IsActive),
                ["Tổng cộng"] = allUsers.Count()
            };
            return stats;
        }

        public async Task<IEnumerable<User>> GetRecentlyActiveUsersAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            return await _unitOfWork.Users.FindAsync(u => 
                u.IsActive && u.LastLogin.HasValue && u.LastLogin.Value >= cutoffDate);
        }

        // Private helper methods
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }
    }
}