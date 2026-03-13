using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthService.InternalModels.DTOs;
using AuthService.Services.Interfaces;
using AuthService.Utils.Common;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(UsersController.GetUser), "Users", new { id = 0 }, result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        var result = await _authService.ResetPasswordAsync(resetPasswordDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var result = await _authService.ChangePasswordAsync(changePasswordDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<string>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        var result = await _authService.RefreshTokenAsync(refreshTokenDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers([FromQuery] SearchQuery searchQuery)
    {
        var result = await _authService.GetUsersAsync(searchQuery);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(int id)
    {
        var result = await _authService.GetUserAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByEmail(string email)
    {
        var result = await _authService.GetUserByEmailAsync(email);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        var result = await _authService.CreateUserAsync(createUserDto);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetUser), new { id = result.Data?.Id ?? 0 }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        var result = await _authService.UpdateUserAsync(id, updateUserDto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id)
    {
        var result = await _authService.DeleteUserAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:int}/activate")]
    public async Task<ActionResult<ApiResponse<string>>> ActivateUser(int id)
    {
        var result = await _authService.ActivateUserAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponse<string>>> DeactivateUser(int id)
    {
        var result = await _authService.DeactivateUserAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}

