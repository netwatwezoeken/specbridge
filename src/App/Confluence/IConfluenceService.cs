namespace App.Confluence;

public interface IConfluenceService
{
    string TableOfContentsMacro { get; }
    Task<ChildrenResponse?> GetChildren(string pageId);
    Task<string> CreatePage(string parentPageId, string title, string content = "");
    Task UpdatePage(string pageId, string title, string content, string reference);
    Task<PageResponse?> GetPage(string pageId);
    Task DeletePage(string pageId, string name, bool recursive = false);
}