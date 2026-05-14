using TherapyCenter.Models;

namespace TherapyCenter.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Case-insensitive email match (used for password recovery when casing may differ).</summary>
        Task<User?> GetByEmailIgnoreCaseAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();

        Task<List<User>> GetByRoleAsync(string role);

        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}