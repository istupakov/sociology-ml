using System.Globalization;

using NewsParser.Data;

using PuppeteerSharp;

namespace NewsParser.Services.Sources;

public class PrecedentTvParser : INewsParser
{
    public string Source => "precedent.tv";

    public async IAsyncEnumerable<string> GetLatestUrls(IPage page)
    {
        await page.GoToAsync($"https://precedent.tv/news");
        for (int i = 1; i <= 100; ++i)
        {
            var urls = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('.rubriks-wrapper h3 a')).map(x => x.href)");
            foreach (var url in urls)
                yield return url;

            var nextUrl = await page.EvaluateExpressionAsync<string?>("Array.from(document.querySelectorAll('.PageNumberer a')).find(x => x.textContent == '>')?.href");
            if (nextUrl is null)
                yield break;

            await page.GoToAsync(nextUrl);
        }
    }

    public async Task<News> Parse(IPage page, string url)
    {
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("h2.news-detail-title");
        var title = await page.EvaluateExpressionAsync<string>("document.querySelector('h2.news-detail-title').textContent.trim()");
        var datetext = await page.EvaluateExpressionAsync<string>("document.querySelector('time').textContent.trim()");
        var content = await page.EvaluateExpressionAsync<string>("Array.from(document.querySelectorAll('.news-detail-summary p, .news-detail-content p')).map(x => x.textContent.trim()).filter(x => x).join('\\n')");
        var date = DateTime.ParseExact(datetext, "dd MMMM yyyy | HH:mm", CultureInfo.GetCultureInfo("ru-RU"));
        return new News { Source = Source, Url = url, Title = title, PublicationTime = date, Content = content };
    }
}
