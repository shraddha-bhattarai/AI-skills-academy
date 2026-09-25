using LearningManagementSystem.Application.DTOs.Auth;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IAuthService
{
    Task<(LoginResponse? Response, string? ErrorMessage)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}
