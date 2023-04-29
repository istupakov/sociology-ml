using NewsParser.Data;

using PuppeteerSharp;

namespace NewsParser.Services.Sources;

public class NgsRuParser : INewsParser
{
    public string Source => "ngs.ru";

    public async IAsyncEnumerable<string> GetLatestUrls(IPage page)
    {
        for (int i = 1; i <= 100; ++i)
        {
            await page.GoToAsync($"https://ngs.ru/text/?page={i}");
            var urls = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('h2 a')).map(x => x.href)");
            foreach (var url in urls.Where(x => !x.Contains("longread")))
                yield return url;
        }
    }

    public async Task<News> Parse(IPage page, string url)
    {
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("h1");
        var title = await page.EvaluateExpressionAsync<string>("document.querySelector('h1').textContent.trim()");
        var date = await page.EvaluateExpressionAsync<DateTime>("document.querySelector('time').dateTime");
        var content = await page.EvaluateExpressionAsync<string>("Array.from(document.querySelectorAll('#record-header p, p:not([class])')).map(x => x.textContent.trim()).filter(x => x).join('\\n')");
        return new News { Source = Source, Url = url, Title = title, PublicationTime = date, Content = content };
    }
}
