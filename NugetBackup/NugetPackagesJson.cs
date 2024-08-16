namespace NugetBackup;

// Автоматически сгенерированные классы с помощью https://json2csharp.com
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

public class Framework
{
    public string framework { get; set; }
    public List<TopLevelPackage> topLevelPackages { get; set; }
    public List<TransitivePackage> transitivePackages { get; set; }
}

public class Project
{
    public string path { get; set; }
    public List<Framework> frameworks { get; set; }
}

public class NugetPackagesJson
{
    public int version { get; set; }
    public string parameters { get; set; }
    public List<Project> projects { get; set; }
}

public class TopLevelPackage
{
    public string id { get; set; }
    public string requestedVersion { get; set; }
    public string resolvedVersion { get; set; }
}

public class TransitivePackage
{
    public string id { get; set; }
    public string resolvedVersion { get; set; }
}

