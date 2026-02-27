using Riff.Api.Data;

namespace Riff.Api.Data.Repositories;

public abstract class BaseRepository
{
    protected readonly AppDbContext Context;

    protected BaseRepository(AppDbContext context)
    {
        Context = context;
    }

    public Task SaveChangesAsync()
    {
        return Context.SaveChangesAsync();
    }
}
