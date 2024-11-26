using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using App.Confluence;
using CommandLine;
using CommandLine.Text;

namespace App;

internal static class Program
{
    private static string _versionString = null!;

    private static async Task Main(string[] args)
    {
        var version = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        _versionString = $"SpecBridge {version!.InformationalVersion}";

        var parser = new Parser(with =>
        {
            with.HelpWriter = null;
            with.CaseInsensitiveEnumValues = true;
        });
        var t =  parser.ParseArguments<Options>(args);
        await t.WithParsedAsync(RunOptions);
        t.WithNotParsed(errs => DisplayHelp(t));
    }

    private static async Task RunOptions(Options opts)
    {
        Console.WriteLine(_versionString);

        var client = new ConfluenceService(new HttpClient(), new ConfluenceServiceConfig()
        {
            BaseUrl = opts.BaseUrl.ToString(),
            SpaceKey = opts.SpaceKey,
            PageId = opts.PageId,
            Username = opts.User,
            Password = opts.Token
        });

        await Processor.DoIt(opts, client);
    }

    private static void HandleParseError(IEnumerable<JSType.Error> errs)
    {
        //handle errors
    }
    
    static void DisplayHelp<T>(ParserResult<T> result)
    {  
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = false;
            h.Heading = _versionString;
            h.Copyright = "Copyright (c) 2024 .NET wat we zoeken";
            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        Console.WriteLine(helpText);
    }
}