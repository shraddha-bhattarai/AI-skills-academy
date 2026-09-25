namespace LearningManagementSystem.Infrastructure.Services;

public interface IJwtService
{
    string GenerateToken(string userId, string email, IEnumerable<string> roles);
}
