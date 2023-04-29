using PuppeteerSharp;

namespace NewsParser.Data;

public interface INewsParser
{
    public string Source { get; }
    public bool CanParse(string url) => new Uri(url).Host == Source;

    public IAsyncEnumerable<string> GetLatestUrls(IPage page);
    public Task<News> Parse(IPage page, string url);
}
