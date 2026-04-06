using Dapper;
using AuthService.Data;
using AuthService.InternalModels.Entities;
using AuthService.Repository;
using AuthService.Repository.Interfaces;
using AuthService.Utils.Common;
using System.Data;

namespace AuthService.Repository.Implementations;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public Task<UserEntity?> GetByIdAsync(int id)
    {
        return ExecuteWithConnectionAsync(async connection =>
            await connection.QuerySingleOrDefaultAsync<UserEntity>(
                StoredProcedureNames.GetUserById,
                new { Id = id },
                commandType: CommandType.StoredProcedure));
    }

    public Task<UserEntity?> GetByEmailAsync(string email)
    {
        return ExecuteWithConnectionAsync(async connection =>
            await connection.QuerySingleOrDefaultAsync<UserEntity>(
                StoredProcedureNames.GetUserByEmail,
                new { Email = email },
                commandType: CommandType.StoredProcedure));
    }

    public async Task<PagedResult<UserEntity>> GetPagedAsync(SearchQuery searchQuery)
    {
        return await ExecuteWithConnectionAsync(
            async connection =>
            {
                using var grid = await connection.QueryMultipleAsync(
                    StoredProcedureNames.GetUsersPaged,
                    new
                    {
                        searchQuery.PageNumber,
                        searchQuery.PageSize,
                        SearchTerm = searchQuery.SearchTerm
                    },
                    commandType: CommandType.StoredProcedure);

                var items = (await grid.ReadAsync<UserEntity>()).ToList();
                var total = await grid.ReadFirstAsync<int>();
                return new PagedResult<UserEntity>(items, total, searchQuery.PageNumber, searchQuery.PageSize);
            });
    }

    public async Task<int> CreateAsync(UserEntity user)
    {
        return await ExecuteWithConnectionAsync(
            async connection =>
            {
                var id = await connection.ExecuteScalarAsync<int>(
                    StoredProcedureNames.CreateUser,
                    user,
                    commandType: CommandType.StoredProcedure);
                user.Id = id;
                return id;
            });
    }

    public async Task<bool> UpdateAsync(UserEntity user)
    {
        return await ExecuteWithConnectionAsync(
            async connection =>
                await connection.ExecuteAsync(
                    StoredProcedureNames.UpdateUser,
                    user,
                    commandType: CommandType.StoredProcedure) > 0);
    }

    public async Task<bool> UpdateLastLoginAsync(int id, DateTime lastLogin)
    {
        return await ExecuteWithConnectionAsync(
            async connection =>
                await connection.ExecuteAsync(
                    StoredProcedureNames.UpdateUserLastLogin,
                    new { Id = id, LastLogin = lastLogin, UpdatedAt = DateTime.UtcNow },
                    commandType: CommandType.StoredProcedure) > 0);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await ExecuteWithConnectionAsync(
            async connection =>
                await connection.ExecuteAsync(
                    StoredProcedureNames.DeleteUser,
                    new { Id = id },
                    commandType: CommandType.StoredProcedure) > 0);
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        return await ExecuteWithConnectionAsync(
            async connection =>
                await connection.ExecuteAsync(
                    StoredProcedureNames.SetUserActiveStatus,
                    new { Id = id, IsActive = isActive, UpdatedAt = DateTime.UtcNow },
                    commandType: CommandType.StoredProcedure) > 0);
    }
}
