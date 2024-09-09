using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExtensionsTesting.repo;

public class Workflow
{
    private readonly IArticleRepository _repository;
    private readonly ILogger _logger;
    public Workflow(IArticleRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }
    public async Task Run(List<ApiBaseArticle> apiBaseArticles)
    {
        foreach (var baseArticle in apiBaseArticles)
        {
            var dbArticle = await _repository.TryGetBaseArticle(baseArticle.Ean);
            if (dbArticle is null)
            {
                dbArticle = await _repository.AddNewBaseArticle(baseArticle);
                _logger.LogInformation("Adding new Base article {BaseEan}", baseArticle.Ean);
            }
            
            // handle set logic:
            foreach (var set in baseArticle.Sets)
            {
                if (dbArticle.SetArticleExists(set))
                {
                    if (dbArticle.IsLegacySetWithMissingComposition(set))
                    {
                        _logger.LogInformation("Adding new SetComposition: {ean} for BaseArticle: {BaseEan}", set.Ean, baseArticle.Ean);
                        dbArticle.AddSetOnly(set);
                    }
                    
                    _logger.LogInformation("Updated SetArticle: {ean} for BaseArticle: {BaseEan}", set.Ean, baseArticle.Ean);
                    dbArticle.UpdateOrAddSetArticle(set);
                }
                else
                {
                    dbArticle.UpdateOrAddSetArticle(set);
                    _logger.LogInformation("Adding new SetArticle: {ean} for BaseArticle: {BaseEan}", set.Ean, baseArticle.Ean);
                }
            }

            _logger.LogInformation("saving all Changes regarding Base-Article: {BaseEan} and corresponding sets.", baseArticle.Ean);
            await dbArticle.SaveToDb();
        }
    }
}

public interface IArticleRepository
{
    public Task<BaseArticle?> TryGetBaseArticle(string ean);
    public Task<BaseArticle> AddNewBaseArticle(ApiBaseArticle baseArticle);
}

public class ArticleRepository: IArticleRepository
{
    private readonly MyBaseContext _ctx;

    public ArticleRepository(MyBaseContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<BaseArticle?> TryGetBaseArticle(string ean)
    {
        return await ActualBaseArticle.Init(_ctx, ean);
    }

    public async Task<BaseArticle> AddNewBaseArticle(ApiBaseArticle baseArticle)
    {
        _ctx.Articles.Add(ConvertBaseArticle(baseArticle));
        return (await TryGetBaseArticle(baseArticle.Ean))!;
    }
    public static Article ConvertBaseArticle(ApiBaseArticle article) => new Article
    {
        Ean = article.Ean,
        Sku = article.Sku,
        Matchcode = null,
        BaseArticles = null,
        ContainedInSets = null
    };
}

public abstract class BaseArticle
{
    public abstract Task SaveToDb();

    public abstract bool SetArticleExists(ApiSet set);
    public abstract void AddSetOnly(ApiSet set);

    public abstract void UpdateOrAddSetArticle(ApiSet set);

    public abstract bool IsLegacySetWithMissingComposition(ApiSet set);
}

public class ActualBaseArticle : BaseArticle
{
    private readonly MyBaseContext _context;
    private readonly Article _baseArticle;

    private ActualBaseArticle(MyBaseContext context, Article a)
    {
        _context = context;
        _baseArticle = a;
    }
    public static async Task<ActualBaseArticle?> Init(MyBaseContext ctx, string Ean)
    {
        var article = await ctx.Articles.Include(a => a.ContainedInSets).ThenInclude(s => s.BaseArticle).SingleOrDefaultAsync(a => a.Sku == Ean);
        if (article is null)
        {
            return null;
        }
        return new ActualBaseArticle(ctx, article);
    }
    public override Task SaveToDb()
    {
        return _context.SaveChangesAsync();
    }
    public override bool SetArticleExists(ApiSet setFromAPi)
    {
        return _baseArticle.ContainedInSets.Any(set => set.BaseArticle.Ean == setFromAPi.Ean);
    }
    public override void AddSetOnly(ApiSet set)
    {
        _baseArticle.ContainedInSets.Add(new SetComp
        {
            BaseSku = _baseArticle.Sku,
            SetSku = set.Sku,
            Count = set.Count,
        });
    }

    public override void UpdateOrAddSetArticle(ApiSet set)
    {
        var article = _baseArticle.ContainedInSets.SingleOrDefault(c => c.SetSku == set.Sku)?.SetArticle;
        if (article is not null)
        {
            article.Sku = _baseArticle.Sku;
            article.Ean = _baseArticle.Ean;
            article.Matchcode = _baseArticle.Matchcode;
        }
        else
        {
            _baseArticle.ContainedInSets.Add(new SetComp()
            {
                BaseArticle = _baseArticle,
                SetArticle = new Article
                {
                    Ean = set.Ean,
                    Sku = set.Sku,
                    Matchcode = _baseArticle.Matchcode,
                },
                Count = set.Count,
            });
        }
    }

    public override bool IsLegacySetWithMissingComposition(ApiSet set)
    {
        return _baseArticle.ContainedInSets.Where(s => s.SetSku == set.Sku).Any(s => s.BaseSku != _baseArticle.Sku);
    }
}