using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<ApplicationUser>()
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new LookupItemDto
            {
                Id = u.Id,
                Name = u.FirstName + " " + u.LastName + " (" + u.Email + ")"
            })
            .ToListAsync(cancellationToken);
    }
}
