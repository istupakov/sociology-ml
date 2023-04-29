using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NewsParser.Data;

namespace NewsParser.Controllers;

[ApiController]
[Route("[controller]")]
public class NewsController : ControllerBase
{
    private readonly NewsContext _newsContext;

    public NewsController(NewsContext newsContext)
    {
        _newsContext = newsContext;
    }

    [HttpGet(Name = "GetNews")]
    public IAsyncEnumerable<News> Get()
    {
        return _newsContext.News.OrderByDescending(x => x.PublicationTime).AsAsyncEnumerable();
    }

    [HttpGet("count", Name = "GetNewsCount")]
    public Task<int> GetCount()
    {
        return _newsContext.News.CountAsync();
    }
}
