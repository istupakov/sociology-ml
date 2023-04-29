using NewsParser.Data;

using PuppeteerSharp;

namespace NewsParser.Services.Sources;

public class SibkrayRuParser : INewsParser
{
    public string Source => "sibkray.ru";

    public async IAsyncEnumerable<string> GetLatestUrls(IPage page)
    {
        for (int i = 1; i <= 100; ++i)
        {
            await page.GoToAsync($"https://sibkray.ru/news/?page={i}");
            var urls = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('.js-news-content h3 a')).map(x => x.href)");
            foreach (var url in urls)
                yield return url;
        }
    }

    public async Task<News> Parse(IPage page, string url)
    {
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("h1");
        var title = await page.EvaluateExpressionAsync<string>("document.querySelector('h1').textContent.trim()");
        var date = await page.EvaluateExpressionAsync<DateTime>("document.querySelector('time').dateTime");
        var content = await page.EvaluateExpressionAsync<string>("document.querySelector('article div[itemprop=\"articleBody\"]').innerText.trim()");
        return new News { Source = Source, Url = url, Title = title, PublicationTime = date, Content = content };
    }
}
