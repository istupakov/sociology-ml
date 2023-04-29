using Microsoft.EntityFrameworkCore;

namespace NewsParser.Data;

public class NewsContext : DbContext
{
    protected NewsContext()
    {
    }

    public NewsContext(DbContextOptions<NewsContext> options) : base(options) { }

    public DbSet<News> News { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<News>().HasAlternateKey(x => x.Url);
        builder.Entity<News>().HasIndex(x => x.Source);
        builder.Entity<News>().HasIndex(x => x.PublicationTime).IsDescending();
    }
}
