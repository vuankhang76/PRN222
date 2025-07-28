using InfertilityApp.Models;

namespace InfertilityApp.BusinessLogicLayer.Interfaces
{
    public interface IUserService
    {
        // CRUD cơ bản
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);

        // Authentication & Authorization
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task<bool> ResetPasswordForOldHashAsync(string username, string newPassword);
        Task<bool> IsUserInRoleAsync(int userId, string role);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        
        // User management
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> UpdateUserRoleAsync(int userId, string newRole);
        Task<bool> UpdateLastLoginAsync(int userId);
        
        // Validation và Security
        Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);
        Task<bool> ValidatePasswordComplexityAsync(string password);
        Task<bool> ValidateUserDataAsync(User user);
        
        // Statistics
        Task<int> GetTotalUsersCountAsync();
        Task<Dictionary<string, int>> GetUsersByRoleStatisticsAsync();
        Task<Dictionary<string, int>> GetActiveUsersStatisticsAsync();
        Task<IEnumerable<User>> GetRecentlyActiveUsersAsync(int days = 30);
    }
} 