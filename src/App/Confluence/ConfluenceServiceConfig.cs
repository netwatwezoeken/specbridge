namespace App.Confluence;

public class ConfluenceServiceConfig
{
    public string BaseUrl { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string PageId { get; set; } = null!;
    public string SpaceKey { get; set; } = null!;
    public int ChildLimit { get; set; } = 25;
}