using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FileBasedDbUnitTesting;

public class PostgresEnumTestingCtx : MyDbCtx
{
    public DbSet<Models> Models { get; set; }

    public PostgresEnumTestingCtx(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresEnum<Material>();
        modelBuilder.Entity<Models>().HasData(new Models
            {
                Id = 1,
                Name = "Firetruck",
                Material = Material.Wood
            },
            new Models
            {
                Id = 2,
                Name = "Paper plane",
                Material = Material.Paper
            }, new Models
            {
                Id = 3,
                Name = "A stone",
                Material = Material.Stone
            });
    }
}

public class Postgresqlconn : PostgresEnumTestingCtx
{
    public Postgresqlconn(DbContextOptions options) : base(options)
    {
    }

    public static Postgresqlconn Create()
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder("User ID=citizix_user;Password=S3cret;Server=127.0.0.1;Port=5432;Database=citizix_db;Pooling=true;");
        dataSourceBuilder.MapEnum<Material>();
        var dataSource = dataSourceBuilder.Build();

        var services = new ServiceCollection();
        services.AddDbContext<Postgresqlconn>(options 
            => options.UseNpgsql(dataSource));
        var s = services.BuildServiceProvider();
        var ctx =  s.GetRequiredService<Postgresqlconn>();
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();
        return ctx;
    }
}

public class Models
{
    [Key] public int Id { get; set; }
    public string Name { get; set; }
    public Material Material { get; set; }
}

public enum Material
{
    Paper,
    Wood,
    Stone,
    Bamboo,
    Wool,
}