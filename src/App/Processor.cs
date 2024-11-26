using App.Confluence;

namespace App;

public static class Processor
{
    public static async Task DoIt(Options opts, IConfluenceService client)
    {
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
        
        await UpdateMainPage(opts, client);
    }

    private static async Task UpdateMainPage(Options opts, IConfluenceService client)
    {
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

    public static async Task Export(FeatureDirectory workingDir,
        IConfluenceService confluenceService, string basePage, string comment)
    {
        var subPages = await confluenceService.GetChildren(basePage);
        
        await DeleteFeaturesThatDoNotExistOnDisk(workingDir, confluenceService, subPages);
        
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
    
    private static async Task DeleteFeaturesThatDoNotExistOnDisk(FeatureDirectory workingDir,
        IConfluenceService confluenceService, ChildrenResponse? subPages)
    {
        foreach (var page in subPages!.results)
        {
            if (!workingDir.Files.Select(file => file.Document.Title).Contains(page.title)
                && !workingDir.Directories.Select(file => file.Name).Contains(page.title))
            {
                await confluenceService.DeletePage(page.id, page.title, true);
            }
        }
    }

    private static async Task<string> CreateFolderPage(FeatureDirectory directory, IConfluenceService confluenceService, string parentPageId)
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
}