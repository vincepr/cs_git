using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FileBasedDbUnitTesting;

public class FileBasedContextFactoryWithPostgreEnums : IDbContextFactory<Postgresqlconn>
{
    private DbContextOptions<Postgresqlconn> _options;
    
    private FileBasedContextFactoryWithPostgreEnums(DbContextOptions<Postgresqlconn> options)
    {
        _options = options;
    }

    public static async Task<FileBasedContextFactoryWithPostgreEnums> New()
    {
        var opts = new DbContextOptionsBuilder<Postgresqlconn>();
        opts.UseSqlite($"Data Source={Path.GetTempFileName()}.sqlite");
        var factory =  new FileBasedContextFactoryWithPostgreEnums(opts.Options);
        await factory.CreateDbContext().Database.EnsureDeletedAsync();
        await factory.CreateDbContext().Database.EnsureCreatedAsync();
        return factory;
    }
    
    public Postgresqlconn CreateDbContext()
    {
        return new Postgresqlconn(_options);
    }
}
public class WillTestsRunWithEnums
{
    private IDbContextFactory<Postgresqlconn> _factory;

    [SetUp]
    public async Task SetUp()
    {
        _factory = await FileBasedContextFactoryWithPostgreEnums.New();
    }
    
    [Test]
    public async Task EnumFromDb()
    {
        using var ctx = await _factory.CreateDbContextAsync();
        var customers = ctx.Models;
        foreach (var c in customers)
        {
            Console.WriteLine(c.Name);
            Console.WriteLine(c.Material);
        }
    }

    [Test]
    public async Task AddEnumValueToDb()
    {
        using var ctx = await _factory.CreateDbContextAsync();
        ctx.Models.Add(new Models
        {
            Name = "Bamboo Sword",
            Material = Material.Bamboo
        });
        var rows = await ctx.SaveChangesAsync();
        rows.Should().Be(1);
        
        using var ctx2 = await _factory.CreateDbContextAsync();
        var customers = ctx.Models;
        foreach (var c in customers)
        {
            Console.WriteLine(c.Name);
            Console.WriteLine(c.Material);
        }
    }
    
}
