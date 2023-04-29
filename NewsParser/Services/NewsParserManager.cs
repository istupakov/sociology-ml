using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;

using NewsParser.Services.Sources;

using PuppeteerSharp;

namespace NewsParser.Data;

public class NewsParserManager : BackgroundService
{
    private readonly IDbContextFactory<NewsContext> _contextFactory;
    private readonly ILogger<NewsParserManager> _logger;
    private readonly List<string> _conflictUrls = new();

    public class Link
    {
        [LoadColumn(3)]
        public string Url { get; set; } = null!;
    }


    public NewsParserManager(IDbContextFactory<NewsContext> contextFactory, ILogger<NewsParserManager> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    private void LoadConflicts()
    {
        var mlContext = new MLContext();
        var loadedData = mlContext.Data.LoadFromTextFile<Link>("conflicts.csv", separatorChar: ',', hasHeader: true, allowQuoting: true);
        var loadedDataEnumerable = mlContext.Data.CreateEnumerable<Link>(loadedData, reuseRowObject: false);
        _conflictUrls.AddRange(loadedDataEnumerable.Select(x => x.Url.Trim()));
    }

    private async Task UpdateSource<T>(IBrowser browser, int maxNewsCount, CancellationToken token)
        where T : INewsParser, new()
    {
        await using var page = await browser.NewPageAsync();
        await page.SetViewportAsync(new ViewPortOptions { Width = 500, Height = 1000 });
        using var context = await _contextFactory.CreateDbContextAsync(token);
        var parser = new T();

        _logger.LogInformation("Updating news for {source}", parser.Source);

        var urls = new List<string>();
        try
        {
            await foreach (var url in parser.GetLatestUrls(page))
            {
                token.ThrowIfCancellationRequested();
                if (urls.Count >= maxNewsCount)
                    break;
                urls.Add(url);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetLatestUrls for {source}", parser.Source);
        }

        urls.AddRange(_conflictUrls.Where(parser.CanParse));
        var existedUrls = await context.News.Where(x => urls.Contains(x.Url)).Select(x => x.Url).ToListAsync(token);
        var newUrls = urls.Except(existedUrls).ToList();
        _logger.LogInformation("Got {count} urls with {newCount} new urls for {source}", urls.Distinct().Count(), newUrls.Count, parser.Source);

        int i = 1;
        foreach (var url in newUrls)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                _logger.LogDebug("Parse {i}/{count} {url}", i++, newUrls.Count, url);
                var news = await parser.Parse(page, url);
                _logger.LogTrace("""
                    News:
                    {title}
                    {date}

                    {content}
                    """, news.Title, news.PublicationTime, news.Content);

                if (string.IsNullOrWhiteSpace(news.Title) || string.IsNullOrWhiteSpace(news.Content))
                    throw new InvalidDataException("Parse result invalid!");

                context.Add(news);
                await context.SaveChangesAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Parse {url} for {source}", url, parser.Source);
            }
        }

        _logger.LogInformation("Updated news for {source}", parser.Source);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //LoadConflicts();

        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

        while (!stoppingToken.IsCancellationRequested)
        {
            const int maxNewsCount = 100;
            await UpdateSource<NgsRuParser>(browser, maxNewsCount, stoppingToken);
            await UpdateSource<PrecedentTvParser>(browser, maxNewsCount, stoppingToken);
            await UpdateSource<SibkrayRuParser>(browser, maxNewsCount, stoppingToken);
            await UpdateSource<TaygaInfoParser>(browser, maxNewsCount, stoppingToken);
            await UpdateSource<AtasInfoParser>(browser, maxNewsCount, stoppingToken);
            await UpdateSource<NdnInfoParser>(browser, maxNewsCount, stoppingToken);

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
