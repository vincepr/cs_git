using Microsoft.EntityFrameworkCore;

namespace FileBasedDbUnitTesting;

public class FileBasedContextFactory : IDbContextFactory<MyDbCtx>
{
    private DbContextOptions<MyDbCtx> _options;
    
    private FileBasedContextFactory(DbContextOptions<MyDbCtx> options)
    {
        _options = options;
    }

    public static async Task<FileBasedContextFactory> New()
    {
        var opts = new DbContextOptionsBuilder<MyDbCtx>();
        opts.UseSqlite($"Data Source={Path.GetTempFileName()}.sqlite");
        var factory =  new FileBasedContextFactory(opts.Options);
        await factory.CreateDbContext().Database.EnsureDeletedAsync();
        await factory.CreateDbContext().Database.EnsureCreatedAsync();
        return factory;
    }
    
    public MyDbCtx CreateDbContext()
    {
        return new MyDbCtx(_options);
    }
}