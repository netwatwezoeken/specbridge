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

        var tree = new FeatureDirectory("root", "", [], []);
        foreach (var path in opts.FeaturePaths)
        {
            var featurePath = path.ToAbsolutePath();
            var baseDir = new DirectoryInfo(featurePath);
            var files = Directory.GetFiles(baseDir.FullName, "*.feature", SearchOption.AllDirectories);
            await IndexDirectory(files, baseDir, tree);
        }

        var workingDir = tree;
        
        await Export(workingDir, client, opts.PageId, opts.Reference);
        
        var page = await client.GetPage(opts.PageId);
        if (page != null)
        {
            var updateRef = opts.Reference == "" ? "no reference" : opts.Reference;
            var content = $"<p>last update: {updateRef}</p>";
            content += client.TableOfContentsMacro;
            await client.UpdatePage(page.id, page.title, content, opts.Reference);
        }
    }

    private static async Task IndexDirectory(string[] files, DirectoryInfo baseDir, FeatureDirectory tree)
    {
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.Directory == null) continue;
            
            var dir = fileInfo.Directory.FullName;
            var relativeDir = dir.Replace(baseDir.FullName, "");
            if (relativeDir.StartsWith(Path.DirectorySeparatorChar))
            {
                relativeDir = relativeDir[1..];
            }
            var relativeDirParts = relativeDir.Split(Path.DirectorySeparatorChar);
            
            var currentDirectory = tree;
            var fullname = "";
            if (relativeDirParts.Length == 0
                || (relativeDirParts[0] == "" && relativeDirParts.Length <=1))
            {
                fullname = Path.DirectorySeparatorChar + fileInfo.Name;
                var content = await File.ReadAllTextAsync(fileInfo.FullName);
                using TextReader sr = new StringReader(content);
                var document = new GherkinParser().ParseToConfluence(sr);
                currentDirectory.Files.Add(new FeatureFile(fileInfo.Name, fullname, content, document));
                continue;
            }
            
            for (var i = 0; i <= relativeDirParts.Length - 1; i++)
            {
                var name = relativeDirParts[i];
                if (fullname == "")
                {
                    fullname = name;
                }
                else
                {
                    fullname = fullname + Path.DirectorySeparatorChar + name;
                }
                if (currentDirectory.Directories.All(d => d.Name != name) && name != "")
                {
                    var newDirectory = new FeatureDirectory(name, fullname, [], []);
                    currentDirectory.Directories.Add(newDirectory);
                }

                if (name != "")
                {
                    currentDirectory = currentDirectory.Directories.First(d => d.Name == name);
                }

                if (i == relativeDirParts.Length - 1)
                {
                    fullname = fullname + Path.DirectorySeparatorChar + fileInfo.Name;
                    var content = await File.ReadAllTextAsync(fileInfo.FullName);
                    using TextReader sr = new StringReader(content);
                    var document = new GherkinParser().ParseToConfluence(sr);
                    currentDirectory.Files.Add(new FeatureFile(fileInfo.Name, fullname, content, document));
                }
            }
        }
    }

    private static async Task Export(FeatureDirectory workingDir,
        ConfluenceService confluenceService, string basePage, string comment)
    {
        var subPages = await confluenceService.GetChildren(basePage);
        
        foreach (var document in workingDir.Files.Select(file => file.Document))
        {
            if (subPages!.results.Any(p => p.title == document.Title))
            {
                var page = subPages!.results.First(p => p.title == document.Title);
                if (document.Publish)
                {
                    await confluenceService.UpdatePage(page.id, document.Title, document.Content, comment);
                }
                else
                {
                    await confluenceService.DeletePage(page.id, document.Title);
                }
            }
            else
            {
                if (document.Publish)
                {
                    await confluenceService.CreatePage(basePage, document.Title, document.Content);
                }
                else
                {
                    Console.WriteLine($"Skipping: {document.Title}");
                }
            }
        }
        
        foreach (var directory in workingDir.Directories)
        {
            var pageId = "";
            if (subPages!.results.Any(p => p.title == directory.FullName))
            {
                pageId = subPages!.results.First(p => p.title == directory.FullName).id;
                if (!directory.Files.Any(f => f.Document.Publish))
                {
                    await confluenceService.DeletePage(pageId, directory.FullName, true);
                }
            }
            else
            {
                if (directory.Files.Any(f => f.Document.Publish))
                {
                    pageId = await CreateFolderPage(directory, confluenceService, basePage);
                }
                else
                {
                    Console.WriteLine($"Skipping: {directory.FullName}");
                }
            }

            if (directory.Files.Any(f => f.Document.Publish) || directory.Directories.Any())
            {
                await Export(directory, confluenceService, pageId, comment);
            }
        }
    }

    private static async Task<string> CreateFolderPage(FeatureDirectory directory, ConfluenceService confluenceService, string parentPageId)
    {
        return await confluenceService.CreatePage(parentPageId, directory.FullName);
    }
    
    private static string ToAbsolutePath(this string input)
    {
        var path = Directory.GetCurrentDirectory();
        path = Path.IsPathRooted(input) ?
            input : 
            Path.Join(path, input);
        return path;
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