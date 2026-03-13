using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Collections.Concurrent;
using AuthService.InternalModels.DTOs;
using AuthService.InternalModels.Entities;
using AuthService.Repository.Interfaces;
using AuthService.Services.Interfaces;
using AuthService.Utils.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services.Implementations;

public class AuthAppService : IAuthService
{
    private sealed class RefreshTokenSession
    {
        public int UserId { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsRevoked { get; set; }
    }

    private static readonly ConcurrentDictionary<string, RefreshTokenSession> RefreshTokenStore = new();

    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthAppService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    private string GenerateJwtToken(UserEntity user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.RoleName)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private int GetAccessTokenExpiryMinutes()
    {
        var raw = _configuration["JwtSettings:ExpiryMinutes"];
        return int.TryParse(raw, out var value) && value > 0 ? value : 60;
    }

    private int GetRefreshTokenExpiryDays()
    {
        var raw = _configuration["JwtSettings:RefreshTokenExpiryDays"];
        return int.TryParse(raw, out var value) && value > 0 ? value : 7;
    }

    private string CreateAndStoreRefreshToken(int userId)
    {
        var token = GenerateRefreshToken();
        var hashed = HashToken(token);
        RefreshTokenStore[hashed] = new RefreshTokenSession
        {
            UserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays()),
            IsRevoked = false
        };
        return token;
    }

    private static void CleanupExpiredRefreshTokens()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in RefreshTokenStore)
        {
            if (kvp.Value.ExpiresAtUtc <= now || kvp.Value.IsRevoked)
            {
                RefreshTokenStore.TryRemove(kvp.Key, out _);
            }
        }
    }

    private static TokenResponseDto ToTokenResponse(LoginResponseDto loginResponse, int expiryMinutes) => new()
    {
        AccessToken = loginResponse.Token,
        RefreshToken = loginResponse.RefreshToken,
        TokenType = "Bearer",
        ExpiresIn = expiryMinutes * 60
    };

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (user is null || !user.IsActive || user.Password != loginDto.Password)
        {
            return ApiResponse<LoginResponseDto>.Fail("Invalid email or password");
        }

        user.LastLogin = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var token = GenerateJwtToken(user);
        var refreshToken = CreateAndStoreRefreshToken(user.Id);
        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = UserDto.FromEntity(user)
        }, "Login successful");
    }

    public async Task<ApiResponse<TokenResponseDto>> TokenAsync(OAuthTokenRequestDto requestDto)
    {
        if (requestDto is null || string.IsNullOrWhiteSpace(requestDto.GrantType))
        {
            return ApiResponse<TokenResponseDto>.Fail("grant_type is required");
        }

        var grantType = requestDto.GrantType.Trim().ToLowerInvariant();
        if (grantType == "password")
        {
            if (string.IsNullOrWhiteSpace(requestDto.Username) || string.IsNullOrWhiteSpace(requestDto.Password))
            {
                return ApiResponse<TokenResponseDto>.Fail("username and password are required for password grant");
            }

            var loginResult = await LoginAsync(new LoginDto
            {
                Email = requestDto.Username,
                Password = requestDto.Password
            });

            if (!loginResult.Success || loginResult.Data is null)
            {
                return ApiResponse<TokenResponseDto>.Fail(loginResult.Message);
            }

            return ApiResponse<TokenResponseDto>.Ok(
                ToTokenResponse(loginResult.Data, GetAccessTokenExpiryMinutes()),
                "Token issued");
        }

        if (grantType == "refresh_token")
        {
            if (string.IsNullOrWhiteSpace(requestDto.RefreshToken))
            {
                return ApiResponse<TokenResponseDto>.Fail("refresh_token is required for refresh_token grant");
            }

            var refreshResult = await RefreshTokenAsync(new RefreshTokenDto { RefreshToken = requestDto.RefreshToken });
            if (!refreshResult.Success || refreshResult.Data is null)
            {
                return ApiResponse<TokenResponseDto>.Fail(refreshResult.Message);
            }

            return ApiResponse<TokenResponseDto>.Ok(
                ToTokenResponse(refreshResult.Data, GetAccessTokenExpiryMinutes()),
                "Token refreshed");
        }

        return ApiResponse<TokenResponseDto>.Fail("Unsupported grant_type. Allowed values: password, refresh_token");
    }

    public async Task<ApiResponse<string>> RegisterAsync(RegisterDto registerDto)
    {
        var existing = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existing is not null)
        {
            return ApiResponse<string>.Fail("A user with this email already exists");
        }

        var user = new UserEntity
        {
            Username = registerDto.Email.Split('@')[0],
            Email = registerDto.Email,
            Password = registerDto.Password,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Phone = registerDto.PhoneNumber ?? string.Empty,
            RoleName = registerDto.Role ?? "Patient",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        return ApiResponse<string>.Ok(user.Email, "Registration successful");
    }

    public Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return Task.FromResult(ApiResponse<string>.Ok(resetToken, "If the email exists, a password reset link has been sent."));
    }

    public Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        if (string.IsNullOrWhiteSpace(resetPasswordDto.Token))
        {
            return Task.FromResult(ApiResponse<string>.Fail("Reset token is required"));
        }

        return Task.FromResult(ApiResponse<string>.Ok("Password reset is accepted in this reference implementation."));
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        var user = await _userRepository.GetByEmailAsync(changePasswordDto.Email);
        if (user is null)
        {
            return ApiResponse<string>.Fail("User not found");
        }

        if (user.Password != changePasswordDto.CurrentPassword)
        {
            return ApiResponse<string>.Fail("Current password is incorrect");
        }

        user.Password = changePasswordDto.NewPassword;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return ApiResponse<string>.Ok("Password has been changed successfully");
    }

    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
        {
            return ApiResponse<LoginResponseDto>.Fail("Refresh token is required");
        }

        CleanupExpiredRefreshTokens();

        var hashed = HashToken(refreshTokenDto.RefreshToken);
        if (!RefreshTokenStore.TryGetValue(hashed, out var session))
        {
            return ApiResponse<LoginResponseDto>.Fail("Invalid refresh token");
        }

        if (session.IsRevoked || session.ExpiresAtUtc <= DateTime.UtcNow)
        {
            RefreshTokenStore.TryRemove(hashed, out _);
            return ApiResponse<LoginResponseDto>.Fail("Refresh token has expired or was revoked");
        }

        var user = await _userRepository.GetByIdAsync(session.UserId);
        if (user is null || !user.IsActive)
        {
            RefreshTokenStore.TryRemove(hashed, out _);
            return ApiResponse<LoginResponseDto>.Fail("User is not active");
        }

        session.IsRevoked = true;
        var newRefreshToken = CreateAndStoreRefreshToken(user.Id);
        var newAccessToken = GenerateJwtToken(user);

        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            User = UserDto.FromEntity(user)
        }, "Token refreshed");
    }

    public Task<ApiResponse<string>> RevokeTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
        {
            return Task.FromResult(ApiResponse<string>.Fail("Refresh token is required"));
        }

        var hashed = HashToken(refreshTokenDto.RefreshToken);
        if (RefreshTokenStore.TryGetValue(hashed, out var session))
        {
            session.IsRevoked = true;
            RefreshTokenStore.TryRemove(hashed, out _);
        }

        return Task.FromResult(ApiResponse<string>.Ok("Token revoked"));
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(SearchQuery searchQuery)
    {
        var page = await _userRepository.GetPagedAsync(searchQuery);
        var dto = new PagedResult<UserDto>(page.Items.Select(UserDto.FromEntity).ToList(), page.TotalCount, page.PageNumber, page.PageSize);
        return ApiResponse<PagedResult<UserDto>>.Ok(dto);
    }

    public async Task<ApiResponse<UserDto>> GetUserAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user is null
            ? ApiResponse<UserDto>.Fail("User not found")
            : ApiResponse<UserDto>.Ok(UserDto.FromEntity(user));
    }

    public async Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user is null
            ? ApiResponse<UserDto>.Fail("User not found")
            : ApiResponse<UserDto>.Ok(UserDto.FromEntity(user));
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        var existing = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existing is not null)
        {
            return ApiResponse<UserDto>.Fail("A user with this email already exists");
        }

        var user = new UserEntity
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            Password = createUserDto.Password,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Phone = createUserDto.Phone,
            RoleName = createUserDto.Role,
            IsActive = createUserDto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);
        return ApiResponse<UserDto>.Ok(UserDto.FromEntity(user), "User created successfully");
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return ApiResponse<UserDto>.Fail("User not found");
        }

        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.Phone = updateUserDto.Phone;
        user.RoleName = updateUserDto.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return ApiResponse<UserDto>.Ok(UserDto.FromEntity(user), "User updated successfully");
    }

    public async Task<ApiResponse<string>> DeleteUserAsync(int id)
    {
        var deleted = await _userRepository.DeleteAsync(id);
        return deleted ? ApiResponse<string>.Ok("User deleted successfully") : ApiResponse<string>.Fail("User not found");
    }

    public async Task<ApiResponse<string>> ActivateUserAsync(int id)
    {
        var updated = await _userRepository.SetActiveAsync(id, true);
        return updated ? ApiResponse<string>.Ok("User activated") : ApiResponse<string>.Fail("User not found");
    }

    public async Task<ApiResponse<string>> DeactivateUserAsync(int id)
    {
        var updated = await _userRepository.SetActiveAsync(id, false);
        return updated ? ApiResponse<string>.Ok("User deactivated") : ApiResponse<string>.Fail("User not found");
    }
}
