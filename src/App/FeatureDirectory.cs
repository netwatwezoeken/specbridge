namespace App;

public record FeatureDirectory(string Name, string FullName, List<FeatureDirectory> Directories, List<FeatureFile> Files);