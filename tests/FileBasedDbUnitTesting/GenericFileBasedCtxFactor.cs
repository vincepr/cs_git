using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace FileBasedDbUnitTesting;

public class GenericFileBasedCtxFactor<TCtx> : IDbContextFactory<TCtx> where TCtx : DbContext
{
    private DbContextOptions<TCtx> _options;
    private readonly Func<DbContextOptions<TCtx>, TCtx> _ctxConstructor;

    private GenericFileBasedCtxFactor(DbContextOptions<TCtx> options)
    {
        _options = options;
        _ctxConstructor = CtxFactoryViaReflection();
    }
    
    public static async Task<GenericFileBasedCtxFactor<TCtx>> New()
    {
        var opts = new DbContextOptionsBuilder<TCtx>();
        opts.UseSqlite($"Data Source={Path.GetTempFileName()}.sqlite");
        var factory =  new GenericFileBasedCtxFactor<TCtx>(opts.Options);
        await factory.CreateDbContext().Database.EnsureDeletedAsync();
        await factory.CreateDbContext().Database.EnsureCreatedAsync();
        return factory;
    }

    public Func<DbContextOptions<TCtx>, TCtx> CtxFactoryViaReflection()
    {
        // run it once to ensure it crashes on creation:
        Type type = typeof(TCtx);
        ConstructorInfo? ctor = type.GetConstructor(new[] { typeof(DbContextOptions) });
        object? instance = ctor?.Invoke(new object[] { _options });
        _ = instance as TCtx ?? throw new InvalidOperationException("Reflection failed. Could not locate ctor. Just implement the ContextFactory manually.");
        
        return (opts) => (TCtx)ctor?.Invoke(new object[] { opts })!;


    }

    public TCtx CreateDbContext()
    {
        return _ctxConstructor(_options);
    }
}