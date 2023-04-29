using System.Globalization;

using NewsParser.Data;

using PuppeteerSharp;

namespace NewsParser.Services.Sources;

public class TaygaInfoParser : INewsParser
{
    public string Source => "tayga.info";
    public IFormatProvider _formatProvider;

    public TaygaInfoParser() 
    {
        var cult = (DateTimeFormatInfo)CultureInfo.GetCultureInfo("ru").DateTimeFormat.Clone();
        cult.AbbreviatedMonthNames = cult.AbbreviatedMonthNames.Select(x => x.Substring(0, x.Length > 3 ? 3 : x.Length)).ToArray();
        _formatProvider = cult;
    }

    public async IAsyncEnumerable<string> GetLatestUrls(IPage page)
    {
        for (int i = 1; i <= 100; ++i)
        {
            await page.GoToAsync($"https://tayga.info/type/1?page={i}");
            var urls = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('a.block_full')).map(x => x.href)");
            foreach (var url in urls)
                yield return url;
        }
    }

    public async Task<News> Parse(IPage page, string url)
    {
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("h1");
        var title = await page.EvaluateExpressionAsync<string>("document.querySelector('h1').textContent.trim()");
        var datetext = await page.EvaluateExpressionAsync<string>("document.querySelector('.news_date').textContent.trim()");
        var content = await page.EvaluateExpressionAsync<string>("Array.from(document.querySelectorAll('h2, p')).map(x => x.textContent.trim()).filter(x => x).join('\\n')");
        var date = DateTime.ParseExact(datetext, "dd MMM yyyy, HH:mm", _formatProvider);
        return new News { Source = Source, Url = url, Title = title, PublicationTime = date, Content = content };
    }
}
