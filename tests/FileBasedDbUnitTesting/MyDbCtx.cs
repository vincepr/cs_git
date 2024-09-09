using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FileBasedDbUnitTesting;

public class MyExtendedCtx : MyDbCtx
{
    public MyExtendedCtx(DbContextOptions options) : base(options) { }
    
    public DbSet<MyExtensions> Extensions { get; set; }
    
    public class MyExtensions
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
    }
}

public class MyDbCtx : DbContext
{
    public MyDbCtx(DbContextOptions options) : base(options) { }
    
    public DbSet<Transaction> Transaction { get; set; }
    
    public DbSet<Customer> Customers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = 1,
                Name = "James Jameson",
                Address = "Jamestown 1123, 142. James Street"
            },
            new Customer
            {
                Id = 2,
                Name = "Bob Ross",
                Address = "ArtistTown 3334, 43. Artist-Street"
            }
        );
        base.OnModelCreating(modelBuilder);
    }
}

public class Transaction
{
    [Key] public int Id { get; set; }
    
    public decimal Price { get; set; }
    
    [ForeignKey(nameof(Customer))] public int CustomerId { get; set; }
    
    public Customer Customer { get; set; }
}

public class Customer
{
    [Key] public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}
