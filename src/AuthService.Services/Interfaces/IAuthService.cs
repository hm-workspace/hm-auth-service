using AuthService.InternalModels.DTOs;
using AuthService.Utils.Common;

namespace AuthService.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponse<TokenResponseDto>> TokenAsync(OAuthTokenRequestDto requestDto);
    Task<ApiResponse<string>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    Task<ApiResponse<string>> RevokeTokenAsync(RefreshTokenDto refreshTokenDto);

    Task<ApiResponse<PagedResult<UserDto>>> GetUsersAsync(SearchQuery searchQuery);
    Task<ApiResponse<UserDto>> GetUserAsync(int id);
    Task<ApiResponse<UserDto>> GetUserByEmailAsync(string email);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<ApiResponse<UserDto>> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<ApiResponse<string>> DeleteUserAsync(int id);
    Task<ApiResponse<string>> ActivateUserAsync(int id);
    Task<ApiResponse<string>> DeactivateUserAsync(int id);
}
