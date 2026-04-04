using AuthService.InternalModels.Entities;
using AuthService.Utils.Common;

namespace AuthService.Repository.Interfaces;

public interface IUserRepository
{
    Task<UserEntity?> GetByIdAsync(int id);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<PagedResult<UserEntity>> GetPagedAsync(SearchQuery searchQuery);
    Task<int> CreateAsync(UserEntity user);
    Task<bool> UpdateAsync(UserEntity user);
    Task<bool> UpdateLastLoginAsync(int id, DateTime lastLogin);
    Task<bool> DeleteAsync(int id);
    Task<bool> SetActiveAsync(int id, bool isActive);
}
