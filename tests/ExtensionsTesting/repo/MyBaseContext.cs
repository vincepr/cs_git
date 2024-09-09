using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ExtensionsTesting.repo;

public class MyBaseContext : DbContext
{
    public MyBaseContext(DbContextOptions options): base(options) { }
    public DbSet<Article> Articles { get; set; }

}

public class Article
{
    public string Ean { get; set; }
    [Key] public string Sku { get; set; }
    public string Matchcode  { get; set; }
    public List<SetComp> BaseArticles { get; set; }
    public List<SetComp> ContainedInSets { get; set; }
}

public class SetComp
{
    [ForeignKey(nameof(BaseArticle))]
    public string BaseSku { get; set; }
    
    public Article BaseArticle { get; set; }
    
    [ForeignKey(nameof(SetArticle))]
    public string SetSku { get; set; }
    
    public Article SetArticle { get; set; }
    
    public int Count { get; set; }
}

public record ApiBaseArticle
{
    public string Ean;
    public string Sku;
    public string Material;
    public List<ApiSet> Sets;
}

public record ApiSet
{
    public string Ean;
    public string Sku;
    public int Count;
}
