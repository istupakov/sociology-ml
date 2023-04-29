namespace NewsParser.Data;

public class News
{
    public Guid Id { get; set; }
    public string Source { get; set; } = null!;
    public string Url { get; set; } = null!;
    public DateTime PublicationTime { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
}
