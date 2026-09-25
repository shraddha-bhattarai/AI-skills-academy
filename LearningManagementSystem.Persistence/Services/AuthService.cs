using LearningManagementSystem.Application.DTOs.Auth;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Infrastructure.Configuration;
using LearningManagementSystem.Infrastructure.Services;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LearningManagementSystem.Persistence.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<(LoginResponse? Response, string? ErrorMessage)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            user = await _userManager.FindByNameAsync(request.Email);
        }

        if (user is null || !user.IsActive)
        {
            return (null, "Invalid email or password.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                return (null, "Account locked. Please try again later.");
            }

            return (null, "Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.Email!, roles);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        await _signInManager.SignInAsync(user, request.RememberMe);

        return (new LoginResponse
        {
            Token = token,
            Email = user.Email!,
            UserId = user.Id,
            ExpiresAt = expiresAt
        }, null);
    }
}
