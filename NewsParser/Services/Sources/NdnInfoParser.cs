using System.Globalization;

using NewsParser.Data;

using PuppeteerSharp;

namespace NewsParser.Services.Sources;

public class NdnInfoParser : INewsParser
{
    public string Source => "ndn.info";

    public async IAsyncEnumerable<string> GetLatestUrls(IPage page)
    {
        for (int i = 1; i <= 100; ++i)
        {
            await page.GoToAsync($"https://ndn.info/novosti/page/{i}/");
            var urls = await page.EvaluateExpressionAsync<string[]>("Array.from(document.querySelectorAll('.jeg_postblock_3 h3 a')).map(x => x.href)");
            foreach (var url in urls)
                yield return url;
        }
    }

    public async Task<News> Parse(IPage page, string url)
    {
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("h1");
        var title = await page.EvaluateExpressionAsync<string>("document.querySelector('h1').textContent.trim()");
        var datetext = await page.EvaluateExpressionAsync<string>("document.querySelector('.jeg_meta_date').textContent.trim()");
        var content = await page.EvaluateExpressionAsync<string>("Array.from(document.querySelectorAll('h2.jeg_post_subtitle, .content-inner p:not(.marke-posle-teksta p)')).map(x => x.textContent.trim()).filter(x => x).join('\\n')");
        var date = DateTime.ParseExact(datetext, "dd.MM.yyyy", CultureInfo.InvariantCulture);
        return new News { Source = Source, Url = url, Title = title, PublicationTime = date, Content = content };
    }
}
