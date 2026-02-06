using InternalPortal.Domain.Entities;
using InternalPortal.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InternalPortal.Persistence.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }
}
