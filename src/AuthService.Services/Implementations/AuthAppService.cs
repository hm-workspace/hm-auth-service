using AuthService.InternalModels.DTOs;
using AuthService.InternalModels.Entities;
using AuthService.Repository.Interfaces;
using AuthService.Services.Interfaces;
using AuthService.Utils.Common;

namespace AuthService.Services.Implementations;

public class AuthAppService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthAppService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

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

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = UserDto.FromEntity(user)
        }, "Login successful");
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

    public Task<ApiResponse<string>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
    {
        if (string.IsNullOrWhiteSpace(refreshTokenDto.RefreshToken))
        {
            return Task.FromResult(ApiResponse<string>.Fail("Refresh token is required"));
        }

        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return Task.FromResult(ApiResponse<string>.Ok(token, "Token refreshed"));
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
