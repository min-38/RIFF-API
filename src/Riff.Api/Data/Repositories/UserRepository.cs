using Microsoft.EntityFrameworkCore;
using Riff.Api.Data.Entities;

namespace Riff.Api.Data.Repositories;

public sealed class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public Task<User?> GetByEmailAsync(string email) =>
        Context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByUsernameAsync(string username) =>
        Context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<bool> ExistsByEmailAsync(string email) =>
        Context.Users.AnyAsync(u => u.Email == email);

    public Task<bool> ExistsByUsernameAsync(string username) =>
        Context.Users.AnyAsync(u => u.Username == username);

    public Task AddAsync(User user)
    {
        return Context.Users.AddAsync(user).AsTask();
    }
}
