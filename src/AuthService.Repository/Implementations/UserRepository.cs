using Dapper;
using AuthService.Data;
using AuthService.InternalModels.Entities;
using AuthService.Repository.Interfaces;
using AuthService.Utils.Common;

namespace AuthService.Repository.Implementations;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public Task<UserEntity?> GetByIdAsync(int id)
    {
        const string sql = @"SELECT
    U.Id,
    U.Username,
    U.Email,
    U.Password,
    U.FirstName,
    U.LastName,
    U.Phone,
    R.RoleName AS RoleName,
    U.IsActive,
    U.LastLogin,
    U.CreatedAt,
    U.UpdatedAt
FROM Users U
JOIN [dbo].[roles] R ON U.roleid = R.id
WHERE U.Id = @Id";

        return QuerySingleOrDefaultAsync<UserEntity>(
            sql,
            new { Id = id },
            () => InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == id));
    }

    public Task<UserEntity?> GetByEmailAsync(string email)
    {
        const string sql = @"SELECT 
	                            U.Id, 
                                U.Username, 
                                U.Email, 
                                U.Password, 
                                U.FirstName,
                                U.LastName, 
                                U.Phone, 
                                UR.RoleName AS RoleName, 
	                            U.IsActive, 
                                U.LastLogin, 
	                            U.CreatedAt, 
                                U.UpdatedAt
                            FROM Users U JOIN [dbo].[roles] UR ON U.roleid=UR.id
                            WHERE LOWER(U.Email) = LOWER(@Email)";

        return QuerySingleOrDefaultAsync<UserEntity>(
            sql,
            new { Email = email },
            () => InMemoryAuthStore.Users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
    }

    public async Task<PagedResult<UserEntity>> GetPagedAsync(SearchQuery searchQuery)
    {
        var term = string.IsNullOrWhiteSpace(searchQuery.SearchTerm) ? null : $"%{searchQuery.SearchTerm}%";
        const string dataSql = @"SELECT
    U.Id,
    U.Username,
    U.Email,
    U.Password,
    U.FirstName,
    U.LastName,
    U.Phone,
    R.RoleName AS RoleName,
    U.IsActive,
    U.LastLogin,
    U.CreatedAt,
    U.UpdatedAt
FROM Users U
JOIN [dbo].[roles] R ON U.roleid = R.id
WHERE @Term IS NULL OR U.Email LIKE @Term OR U.FirstName LIKE @Term OR U.LastName LIKE @Term
ORDER BY U.Id
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        const string countSql = @"SELECT COUNT(1)
FROM Users U
WHERE @Term IS NULL OR U.Email LIKE @Term OR U.FirstName LIKE @Term OR U.LastName LIKE @Term";

        var offset = (searchQuery.PageNumber - 1) * searchQuery.PageSize;

        return await ExecuteWithConnectionAsync(
            async connection =>
            {
                var items = (await connection.QueryAsync<UserEntity>(dataSql, new { Term = term, Offset = offset, searchQuery.PageSize })).ToList();
                var total = await connection.ExecuteScalarAsync<int>(countSql, new { Term = term });
                return new PagedResult<UserEntity>(items, total, searchQuery.PageNumber, searchQuery.PageSize);
            },
            () =>
            {
                var query = InMemoryAuthStore.Users.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
                {
                    query = query.Where(x =>
                        x.Email.Contains(searchQuery.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        x.FirstName.Contains(searchQuery.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                        x.LastName.Contains(searchQuery.SearchTerm, StringComparison.OrdinalIgnoreCase));
                }

                var total = query.Count();
                var items = query
                    .OrderBy(x => x.Id)
                    .Skip((searchQuery.PageNumber - 1) * searchQuery.PageSize)
                    .Take(searchQuery.PageSize)
                    .ToList();
                return new PagedResult<UserEntity>(items, total, searchQuery.PageNumber, searchQuery.PageSize);
            });
    }

    public async Task<int> CreateAsync(UserEntity user)
    {
        const string sql = @"INSERT INTO Users
(Username, Email, Password, FirstName, LastName, Phone, RoleId, IsActive, LastLogin, CreatedAt, UpdatedAt)
OUTPUT INSERTED.Id
VALUES
(@Username, @Email, @Password, @FirstName, @LastName, @Phone,
 COALESCE((SELECT TOP 1 Id FROM [dbo].[roles] WHERE LOWER(RoleName) = LOWER(@RoleName)),
          (SELECT TOP 1 Id FROM [dbo].[roles] WHERE LOWER(RoleName) = 'patient')),
 @IsActive, @LastLogin, @CreatedAt, @UpdatedAt)";

        return await ExecuteWithConnectionAsync(
            async connection =>
            {
                var id = await connection.ExecuteScalarAsync<int>(sql, user);
                user.Id = id;
                return id;
            },
            () =>
            {
                user.Id = Interlocked.Increment(ref InMemoryAuthStore.UserSeed);
                InMemoryAuthStore.Users.Add(user);
                return user.Id;
            });
    }

    public async Task<bool> UpdateAsync(UserEntity user)
    {
        const string sql = @"UPDATE Users
SET Username=@Username, Email=@Email, Password=@Password, FirstName=@FirstName, LastName=@LastName,
    Phone=@Phone,
    RoleId=COALESCE((SELECT TOP 1 Id FROM [dbo].[roles] WHERE LOWER(RoleName) = LOWER(@RoleName)), RoleId),
    IsActive=@IsActive, LastLogin=@LastLogin, UpdatedAt=@UpdatedAt
WHERE Id=@Id";

        return await ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, user) > 0,
            () =>
            {
                var existing = InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == user.Id);
                if (existing is null)
                {
                    return false;
                }

                existing.Username = user.Username;
                existing.Email = user.Email;
                existing.Password = user.Password;
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.Phone = user.Phone;
                existing.RoleName = user.RoleName;
                existing.IsActive = user.IsActive;
                existing.LastLogin = user.LastLogin;
                existing.UpdatedAt = user.UpdatedAt;
                return true;
            });
    }

    public async Task<bool> UpdateLastLoginAsync(int id, DateTime lastLogin)
    {
        const string sql = "UPDATE Users SET LastLogin=@LastLogin, UpdatedAt=@UpdatedAt WHERE Id=@Id";
        return await ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, new { Id = id, LastLogin = lastLogin, UpdatedAt = DateTime.UtcNow }) > 0,
            () =>
            {
                var existing = InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == id);
                if (existing is null)
                {
                    return false;
                }
                existing.LastLogin = lastLogin;
                existing.UpdatedAt = DateTime.UtcNow;
                return true;
            });
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Users WHERE Id = @Id";

        return await ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, new { Id = id }) > 0,
            () =>
            {
                var existing = InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == id);
                if (existing is null)
                {
                    return false;
                }

                InMemoryAuthStore.Users.Remove(existing);
                return true;
            });
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        const string sql = "UPDATE Users SET IsActive=@IsActive, UpdatedAt=@UpdatedAt WHERE Id=@Id";

        return await ExecuteWithConnectionAsync(
            async connection => await connection.ExecuteAsync(sql, new { Id = id, IsActive = isActive, UpdatedAt = DateTime.UtcNow }) > 0,
            () =>
            {
                var existing = InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == id);
                if (existing is null)
                {
                    return false;
                }

                existing.IsActive = isActive;
                existing.UpdatedAt = DateTime.UtcNow;
                return true;
            });
    }
}
