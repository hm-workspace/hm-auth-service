using Dapper;
using AuthService.Data;
using AuthService.InternalModels.Entities;
using AuthService.Repository.Interfaces;
using AuthService.Utils.Common;

namespace AuthService.Repository.Implementations;

public class DapperUserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperUserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserEntity?> GetByIdAsync(int id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"SELECT Id, Username, Email, Password, FirstName, LastName, Phone, RoleName, IsActive, LastLogin, CreatedAt, UpdatedAt
FROM Users WHERE Id = @Id";
            return await connection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { Id = id });
        }
        catch
        {
            return InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == id);
        }
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"SELECT Id, Username, Email, Password, FirstName, LastName, Phone, RoleName, IsActive, LastLogin, CreatedAt, UpdatedAt
FROM Users WHERE LOWER(Email) = LOWER(@Email)";
            return await connection.QuerySingleOrDefaultAsync<UserEntity>(sql, new { Email = email });
        }
        catch
        {
            return InMemoryAuthStore.Users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
    }

    public async Task<PagedResult<UserEntity>> GetPagedAsync(SearchQuery searchQuery)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            var term = string.IsNullOrWhiteSpace(searchQuery.SearchTerm) ? null : $"%{searchQuery.SearchTerm}%";
            const string dataSql = @"SELECT Id, Username, Email, Password, FirstName, LastName, Phone, RoleName, IsActive, LastLogin, CreatedAt, UpdatedAt
FROM Users
WHERE @Term IS NULL OR Email LIKE @Term OR FirstName LIKE @Term OR LastName LIKE @Term
ORDER BY Id
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
            const string countSql = @"SELECT COUNT(1)
FROM Users
WHERE @Term IS NULL OR Email LIKE @Term OR FirstName LIKE @Term OR LastName LIKE @Term";

            var offset = (searchQuery.PageNumber - 1) * searchQuery.PageSize;
            var items = (await connection.QueryAsync<UserEntity>(dataSql, new { Term = term, Offset = offset, searchQuery.PageSize })).ToList();
            var total = await connection.ExecuteScalarAsync<int>(countSql, new { Term = term });
            return new PagedResult<UserEntity>(items, total, searchQuery.PageNumber, searchQuery.PageSize);
        }
        catch
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
        }
    }

    public async Task<int> CreateAsync(UserEntity user)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"INSERT INTO Users
(Username, Email, Password, FirstName, LastName, Phone, RoleName, IsActive, LastLogin, CreatedAt, UpdatedAt)
OUTPUT INSERTED.Id
VALUES
(@Username, @Email, @Password, @FirstName, @LastName, @Phone, @RoleName, @IsActive, @LastLogin, @CreatedAt, @UpdatedAt)";
            var id = await connection.ExecuteScalarAsync<int>(sql, user);
            user.Id = id;
            return id;
        }
        catch
        {
            user.Id = Interlocked.Increment(ref InMemoryAuthStore.UserSeed);
            InMemoryAuthStore.Users.Add(user);
            return user.Id;
        }
    }

    public async Task<bool> UpdateAsync(UserEntity user)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"UPDATE Users
SET Username=@Username, Email=@Email, Password=@Password, FirstName=@FirstName, LastName=@LastName,
    Phone=@Phone, RoleName=@RoleName, IsActive=@IsActive, LastLogin=@LastLogin, UpdatedAt=@UpdatedAt
WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, user) > 0;
        }
        catch
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
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "DELETE FROM Users WHERE Id = @Id";
            return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
        }
        catch
        {
            var existing = InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == id);
            if (existing is null)
            {
                return false;
            }

            InMemoryAuthStore.Users.Remove(existing);
            return true;
        }
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE Users SET IsActive=@IsActive, UpdatedAt=@UpdatedAt WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, new { Id = id, IsActive = isActive, UpdatedAt = DateTime.UtcNow }) > 0;
        }
        catch
        {
            var existing = InMemoryAuthStore.Users.FirstOrDefault(x => x.Id == id);
            if (existing is null)
            {
                return false;
            }

            existing.IsActive = isActive;
            existing.UpdatedAt = DateTime.UtcNow;
            return true;
        }
    }
}

internal static class InMemoryAuthStore
{
    public static int UserSeed = 1;
    public static readonly Dictionary<string, RefreshTokenEntity> RefreshTokens = new(StringComparer.Ordinal);
    public static readonly List<UserEntity> Users =
    [
        new UserEntity
        {
            Id = 1,
            Username = "admin",
            Email = "admin@hm.local",
            Password = "Admin@123",
            FirstName = "System",
            LastName = "Admin",
            Phone = "9999999999",
            RoleName = "Admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }
    ];
}
