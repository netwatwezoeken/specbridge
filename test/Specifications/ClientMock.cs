using App.Confluence;

namespace Specifications;

public class ClientMock : IConfluenceService
{
    private List<(string, string, string)> _creates;
    private List<(string, string, string, string)> _updates;
    private List<(string, string, bool)> _deletes;
    public string TableOfContentsMacro { get; } = "ToC macro";
    public List<Page> Pages { get; set; }
    public int Created => _creates.Count;

    public ClientMock(List<Page> pages)
    {
        _creates = new List<(string, string, string)>();
        _updates = new List<(string, string, string, string)>();
        _deletes = new List<(string, string, bool)>();
        Pages = pages.ToList();
    }
    
    public Task<ChildrenResponse?> GetChildren(string pageId)
    {
        var children = Pages
            .Where(p => p.ParentPageId == pageId);
        return Task.FromResult<ChildrenResponse?>(
            new ChildrenResponse(children
                .Select(p => new Results(p.PageId, "", p.Title, "", 1))
                .ToArray())
        );
    }

    public Task<string> CreatePage(string parentPageId, string title, string content = "")
    {
        if (Pages.Any(p => p.Title == title))
        {
            throw new ArgumentException("Title already exists");
        }
        var pageId = Guid.NewGuid().ToString();
        Pages.Add(new Page(pageId, parentPageId, content, title));
        _creates.Add((pageId, title, content));
        return Task.FromResult(pageId);
    }

    public Task UpdatePage(string pageId, string title, string content, string reference)
    {
        var page = Pages.Single(p => p.PageId == pageId);
        Pages.Remove(page);
        page = page with { Title = title, Content = content};
        Pages.Add(page);
        _updates.Add((pageId, title, content, reference));
        return Task.CompletedTask;
    }

    public bool ReceivedUpdatePage(string pageId, string title, string content = null, string reference = null)
    {
        return _updates.Any(u => u.Item1 == pageId && u.Item2 == title);
    }

    public Task<PageResponse?> GetPage(string pageId)
    {
        return Task.FromResult<PageResponse?>(null);
    }
    
    public Task DeletePage(string pageId, string name, bool recursive = false)
    {
        var page = Pages.Single(p => p.PageId == pageId && p.Title == name);
        if (recursive)
        {
            foreach (var childPage in Pages.Where(p => p.ParentPageId == pageId).ToList())
            {
                DeleteChildPages(childPage.PageId, childPage.Title);
            }
        }
        Pages.Remove(page);
        _deletes.Add((pageId, name, recursive));
        return Task.CompletedTask;
    }

    private void DeleteChildPages(string pageId, string name)
    {
        var page = Pages.Single(p => p.PageId == pageId && p.Title == name);
        foreach (var childPage in Pages.Where(p => p.ParentPageId == pageId))
        {
            DeleteChildPages(childPage.PageId, childPage.Title);
        }
        Pages.Remove(page);
    }
    
    public bool ReceivedDeletePage(string pageId, string title, bool recursive = false)
    {
        return _deletes.Any(u => u.Item1 == pageId && u.Item2 == title && u.Item3 == recursive);
    }
}