using CommandLine;

namespace App;

public class Options
{
    [Option('f', "features", Required = false, HelpText = "paths to feature files. default is './'")]
    public required IEnumerable<string> FeaturePaths { get; set; }
    
    [Option("url", Required = true, HelpText = "atlassian base url, for example 'https://nwwz.atlassian.net/'")]
    public required Uri BaseUrl { get; set; }
    
    [Option("space", Required = true, HelpText = "atlassian space key, for example 'SpecBridge'")]
    public required string SpaceKey { get; set; }
        
    [Option("page", Required = true, HelpText = "id of the page under which to place the specifications")]
    public required string PageId { get; set; }
        
    [Option("user", Required = true, HelpText = "username to authenticate with")]
    public required string User { get; set; }
        
    [Option("password", Required = true, HelpText = "password to authenticate with")]
    public required string Password { get; set; }
}

public enum Format
{
    Json,
    MermaidMd
}