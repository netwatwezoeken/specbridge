using App;
using Reqnroll;

namespace Specifications;

[Binding]
public class BasicStep
{
    private ClientMock _confluenceMock = null!;
    private Options _options;

    public BasicStep()
    {
        _options = new Options
        {
            FeaturePaths = [],
            BaseUrl = new Uri("http://localhost:5000"),
            SpaceKey = "",
            PageId = "",
            User = "",
            Token = "",
            Reference = ""
        };
    }

    [Given(@"specification directory (.*)")]
    public void GivenDirectoryTestsBase(string directory)
    {
        _options.FeaturePaths = [directory];
    }

    [When(@"syncing to (.*)")]
    public async Task WhenSyncingThatDirectoryToAnEmptyPage(string page)
    {
        _options.PageId = page; //_confluenceService.Pages.Single(p => p.PageId == page).PageId;
        await Processor.DoIt(_options, _confluenceMock);
    }

    [Then(@"a directory page named (.*) should be created under (.*)")]
    public void ThenADirectoryPageWithNameShouldBeCreated(string title, string parentId)
    {
        Assert.True(_confluenceMock.Pages.Any(
            p => p.Title == title && p.ParentPageId == parentId && p.Content == string.Empty));
    }
    
    [Then(@"a feature page named (.*) should be created under (.*)")]
    public void ThenAFeaturePageWithNameShouldBeCreated(string title, string parentTitle)
    {
        var parent = _confluenceMock.Pages.First(p => p.Title == parentTitle);
        Assert.True(_confluenceMock.Pages.Any(
            p => p.Title == title && p.ParentPageId == parent.PageId));
    }

    [Given(@"these pages exist")]
    public void GivenThesePagesExist(DataTable table)
    {
        var pages = table.CreateSet<Page>().ToList();
        _confluenceMock = new ClientMock(pages);
    }
    
    [Then(@"feature pageId (.*) named (.*) is updated")]
    public void ThenFeaturePageIsUpdated(string pageId, string title)
    {
        Assert.True(_confluenceMock.ReceivedUpdatePage(pageId, title));
    }

    [Then(@"feature pageId (.*) named (.*) is recursively deleted")]
    public void ThenPageIsRecursivelyDeleted(string pageId, string title)
    {
        Assert.True(_confluenceMock.ReceivedDeletePage(pageId, title, true));
    }
    
    [Then(@"feature pageId (.*) named (.*) is deleted")]
    public void ThenFeaturePageIsDeleted(string pageId, string title)
    {
        Assert.True(_confluenceMock.ReceivedDeletePage(pageId, title));
    }

    [Then(@"number of pages created is (.*)")]
    public void ThenNumberOfPagesCreatedIs(int number)
    {
        Assert.Equal(number, _confluenceMock.Created);
    }
}

public record Page(
    string PageId,
    string? ParentPageId,
    string Content,
    string Title);