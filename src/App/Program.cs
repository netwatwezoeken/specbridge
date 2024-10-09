﻿using System.Reflection;
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
            Password = opts.Password
        });
        var t = await client.GetChildren();

        var tree = new FeatureDirectory("root", "", [], []);
        foreach (var path in opts.FeaturePaths)
        {
            var featurePath = path.ToAbsolutePath();
            var baseDir = new DirectoryInfo(featurePath);
            var files = Directory.GetFiles(baseDir.FullName, "*.feature", SearchOption.AllDirectories);
            await IndexDirectory(files, baseDir, tree);
        }

        var workingDir = tree;
        
        await Export(workingDir, client, opts.PageId);
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
                Console.WriteLine($"Adding new file {fileInfo.Name}");
                currentDirectory.Files.Add(new FeatureFile(fileInfo.Name, fullname, content));
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
                    Console.WriteLine($"Adding new directory {newDirectory}");
                }

                if (name != "")
                {
                    currentDirectory = currentDirectory.Directories.First(d => d.Name == name);
                }

                if (i == relativeDirParts.Length - 1)
                {
                    fullname = fullname + Path.DirectorySeparatorChar + fileInfo.Name;
                    var content = await File.ReadAllTextAsync(fileInfo.FullName);
                    Console.WriteLine($"Adding new file {fileInfo.Name}");
                    currentDirectory.Files.Add(new FeatureFile(fileInfo.Name, fullname, content));
                }
            }
        }
    }

    private static async Task Export(FeatureDirectory workingDir, 
        ConfluenceService confluenceService, string basePage)
    {
        var subPages = await confluenceService.GetChildren(basePage);
        
        foreach (var file in workingDir.Files)
        {
            using TextReader sr = new StringReader(file.content);
            var document = new GherkinParser().ParseToConfluence(sr);
            if (subPages!.results.Any(p => p.title == document.Title))
            {
                var page = subPages!.results.First(p => p.title == document.Title);
                await confluenceService.UpdatePage( page.id, document.Title, document.Content);
            }
            else
            {
                await confluenceService.CreatePage(basePage, document.Title, document.Content);
            }
        }
        
        foreach (var directory in workingDir.Directories)
        {
            var pageId = "";
            if (subPages!.results.Any(p => p.title == directory.FullName))
            {
                pageId = subPages!.results.First(p => p.title == directory.FullName).id;
            }
            else
            {
                pageId = await CreateFolderPage(directory, confluenceService, basePage);
            }
            await Export(directory, confluenceService, pageId);
        }
        
        var pagesToRemove = subPages!.results.Where(p => workingDir.Files.All(f => f.FullName != p.title));
        pagesToRemove = pagesToRemove!.Where(p => workingDir.Directories.All(f => f.FullName != p.title));

        foreach (var page in pagesToRemove)
        {
            Console.WriteLine($"Removing page {page.title}");
        }
    }

    private static async Task<string> CreateFolderPage(FeatureDirectory directory, ConfluenceService confluenceService, string parentPageId)
    {
        Console.WriteLine("Create folder: " + directory.Name);
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