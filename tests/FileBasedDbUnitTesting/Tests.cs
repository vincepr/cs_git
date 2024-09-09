using System.Xml.Schema;
using Microsoft.EntityFrameworkCore;

namespace FileBasedDbUnitTesting;

public class Tests
{
    private IDbContextFactory<MyDbCtx> _contextFactory;
    
    private IDbContextFactory<MyDbCtx> _genericFileBased;
    private IDbContextFactory<MyExtendedCtx> _extendedFactory;
    
    [SetUp]
    public async Task Setup()
    {
        _contextFactory = await FileBasedContextFactory.New();
        _genericFileBased = await GenericFileBasedCtxFactor<MyDbCtx>.New();
        _extendedFactory = await GenericFileBasedCtxFactor<MyExtendedCtx>.New();
    }

    private static void AddCustomer(MyDbCtx ctx)
    {
        ctx.Customers.Add(new Customer
        {
            Name = "Fred",
            Address = "End of the world, 123. Somestreet"
        });
    }
    
    [Test]
    public void Test1()
    {
        using var ctx = _contextFactory.CreateDbContext();
        Assert.AreEqual(ctx.Customers.Count(), 2);
    }
    [Test]
    public async Task Test2()
    {
        await using var ctx = _contextFactory.CreateDbContext();
        AddCustomer(ctx);
        await ctx.SaveChangesAsync();
        await using var ctx2 = _contextFactory.CreateDbContext();
        Assert.AreEqual(ctx.Customers.Count(), 3);
    }


    [Test]
    public void Test1_Generic()
    {
        using var ctx = _genericFileBased.CreateDbContext();
        Assert.AreEqual(ctx.Customers.Count(), 2);
    }
    [Test]
    public async Task Test2_Generic()
    {
        await using var ctx = _genericFileBased.CreateDbContext();
        AddCustomer(ctx);
        await ctx.SaveChangesAsync();
        await using var ctx2 = _genericFileBased.CreateDbContext();
        Assert.AreEqual(ctx.Customers.Count(), 3);
    }
    
    
    [Test]
    public void Test1_Extended()
    {
        using var ctx = _extendedFactory.CreateDbContext();
        Assert.AreEqual(ctx.Customers.Count(), 2);
    }
    [Test]
    public async Task Test2_Extended()
    {
        await using var ctx = _extendedFactory.CreateDbContext();
        AddCustomer(ctx);
        await ctx.SaveChangesAsync();
        await using var ctx2 = _extendedFactory.CreateDbContext();
        Assert.AreEqual(ctx.Customers.Count(), 3);
    }
    [Test]
    public async Task Test3_Extended()
    {
        await using var ctx = _extendedFactory.CreateDbContext();
        ctx.Extensions.Add(new MyExtendedCtx.MyExtensions
        {
            Name = "Bob"
        });
        await ctx.SaveChangesAsync();
        await using var ctx2 = _extendedFactory.CreateDbContext();
        Assert.That(ctx.Extensions.Single().Name, Is.EqualTo("Bob"));
    }
}