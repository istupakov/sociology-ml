using System.Globalization;

using NewsParser.Data;

using PuppeteerSharp;

namespace NewsParser.Services.Sources;

public class AtasInfoParser : INewsParser
{
    public string Source => "atas.info";

    public async IAsyncEnumerable<string> GetLatestUrls(IPage page)
    {
        await page.GoToAsync($"https://atas.info/news");
        for (int i = 1; i <= 250; ++i)
        {
            await page.Keyboard.DownAsync(PuppeteerSharp.Input.Key.PageDown);
            await Task.Delay(250);
            await page.WaitForSelectorAsync("div.mb-8 a");
        }

        var urls = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('div.mb-8 a')).map(x => x.href)");
        foreach (var url in urls)
            yield return url;
    }

    public async Task<News> Parse(IPage page, string url)
    {
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("h1");
        var title = await page.EvaluateExpressionAsync<string>("document.querySelector('h1').textContent.trim()");
        var datetext = await page.EvaluateExpressionAsync<string>("document.querySelector('div[class *= \"_date_\"').textContent.trim()");
        //#matter-0 > div.relative.desktop-cols-3 > div:nth-child(2) > div:nth-child(2), 
        var content = await page.EvaluateExpressionAsync<string>("Array.from(document.querySelectorAll('#matter-0 > div.relative.desktop-cols-3 > div:nth-child(2) *:is(p, blockquote)')).map(x => x.textContent.trim()).filter(x => x).join('\\n')");
        var date = DateTime.ParseExact(datetext, "d MMMM yyyy, HH:mm", CultureInfo.GetCultureInfo("ru-RU"));
        return new News { Source = Source, Url = url, Title = title, PublicationTime = date, Content = content };
    }
}
