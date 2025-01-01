using AngleSharp;
using AngleSharp.Dom;
using App;
using Reqnroll;
using VerifyTests.AngleSharp;

namespace Specifications;

[Binding]
public class ParsingSteps
{
    private string _multilineText = string.Empty;
    private ConfluenceDocument _document = null!;
    private readonly IBrowsingContext _htmlContext;

    public ParsingSteps()
    {
        var config = Configuration.Default.WithDefaultLoader();
        _htmlContext = BrowsingContext.New(config);
        if (!VerifyAngleSharpDiffing.Initialized)
        {
            VerifyAngleSharpDiffing.Initialize();
        }
    }

    [When(@"Parsed")]
    public void WhenParsed()
    {
        using TextReader sr = new StringReader(_multilineText);
        _document = new GherkinParser().ParseToConfluence(sr);
    }

    [Given(@"feature file (.*)")]
    public async Task GivenFeatureFileTestsParsingBasicScenarioFeature(string file)
    {
        _multilineText = await File.ReadAllTextAsync(file);
    }
    
    [Then(@"feature title is (.*)")]
    public void ThenFeatureTitleIsUsingAndAndBut(string title)
    {
        Assert.Equal(title, _document.Title);
    }

    [Then(@"result contains a Info Panel with (.*)")]
    public async Task ThenResultContainsAInformationPanelWithSomeText(string text)
    {
        var document = await _htmlContext.OpenAsync(req => req.Content(_document.Content));
        var actualText = document
            .QuerySelector(
                "ac\\:structured-macro[ac\\:macro-id=\"3c9c0069-88dd-4a2b-8fee-076564d6faa8\"] > ac\\:rich-text-body")!
            .Text();
        Assert.Equal(text, actualText);
    }
    
    [Then(@"result contains scenario title (.*)")]
    public void ThenResultContainsScenarioTitle(string text)
    {
        Assert.Contains(text, _document.Content);
    }
    
    [Then(@"result contains a collapsable Code Block with these entries")]
    public async Task ThenResultContainsACollapsableCodeBlockWithArray(DataTable table)
    {
        var entries = table.CreateSet<Entry>().ToList();
        var document = await _htmlContext.OpenAsync(req => req.Content(_document.Content));
        var actualList = document.QuerySelectorAll("ac\\:structured-macro ac\\:rich-text-body > blockquote > p").Select(p => p.Text().Replace("  ", " ")).ToList();
        Assert.Equal(entries.Select(e => e.Line).ToList(), actualList);
    }
    
    [Then(@"result contains a Code Block with these entries")]
    public async Task ThenResultContainsACodeBlockWithArray(DataTable table)
    {
        var entries = table.CreateSet<Entry>().ToList();
        var document = await _htmlContext.OpenAsync(req => req.Content(_document.Content));
        var actualList = document.QuerySelectorAll("blockquote > p").Select(p => p.Text().Replace("  ", " ")).ToList();
        Assert.Equal(entries.Select(e => e.Line).ToList(), actualList);
    }

    [Then(@"result contains a html table with")]
    public async Task ThenContainsATableWith(DataTable dataTable)
    {
        var document = await _htmlContext.OpenAsync(req => req.Content(_document.Content));
        var pages = dataTable.CreateSet<Row>().ToList();
        var tables = document.QuerySelectorAll("table").ToList();
        var tablesInHtml = GetTables(tables);

        Assert.Contains(tablesInHtml, r => IsEqualTo(r, dataTable));
    }

    private static List<List<List<string>>> GetTables(List<IElement> tables)
    {
        var htmlTables = new List<List<List<string>>>();
        foreach (var table in tables)
        {
            var referenceTable = new List<List<string>>();
            var rows = table.QuerySelectorAll("tbody > tr");
            foreach (var row in rows)
            {
                var headerCells = row.QuerySelectorAll("th");
                if (headerCells.Length > 1)
                {
                    referenceTable.Add(headerCells.Select(h =>  h.Text()).ToList());
                }
                else
                {
                    var rowCells = row.QuerySelectorAll("td")
                        .Select(h =>  h.Text()).ToList();
                    referenceTable.Add(rowCells);
                }
            }
            htmlTables.Add(referenceTable);
        }

        return htmlTables;
    }

    private bool IsEqualTo(List<List<string>> list, DataTable dataTable)
    {
        if (!dataTable.Header.ToList().SequenceEqual(list[0]))
        {
            return false;
        }

        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            if (!dataTable.Rows[i].Values.ToList().SequenceEqual(list[i+1]))
            {
                return false;
            }
        }
        
        return dataTable.Rows.Count == list.Count -1;
    }

    [Then(@"result should match the reference")]
    public Task ThenResultShouldMatchTheReference()
    {
        return Verify(_document.Content, "html").PrettyPrintHtml();
    }
}

public record Entry(
    string Line);
    
public record Row(
    string Number1,
    string Number2,
    string Result);