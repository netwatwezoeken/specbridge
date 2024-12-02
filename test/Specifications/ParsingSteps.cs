using App;
using Reqnroll;
using VerifyTests.AngleSharp;

namespace Specifications;

[Binding]
public class ParsingSteps
{
    private string _multilineText = string.Empty;
    private ConfluenceDocument _document = null!;

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

    [Then(@"result matches snapshot")]
    public async Task ThenResultMatchesSnapshot()
    {
        await Verify(_document.Content, "html").PrettyPrintHtml();
    }
}