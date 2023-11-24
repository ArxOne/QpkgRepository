using ArxOne.Qnap.Utility;

namespace ArxOne.Qnap;

public class QpkgPackage
{
    public string Signature { get; private init; }

    public string Name { get; private init; }

    public string DisplayName { get; private init; }

    public Version Version { get; private init; }

    public string Author { get; private init; }

    public string Summary { get; private init; }

    public string FirmwareMinimumVersion { get; private init; }

    public string TutorialLink { get; private init; }

    public string ForumLink { get; private init; }

    public string ChangelogLink { get; private init; }

    public string Category { get; private init; }

    public string Type { get; private init; }

    public string BannerImg { get; private init; }

    public string Icon80Uri { get; private init; }

    public string Icon100Uri { get; private init; }

    public string Languages { get; private init; }

    public string? SnapshotUri { get; private init; }

    public DateTime PublishedDate { get; private init; }

    public Uri Location { get; private init; }

    private QpkgPackage(string signature, string name, string displayName, Version version, string author, string summary, string firmwareMinimumVersion, string tutorialLink, string forumLink, string changelogLink, string category, string type, string bannerImg, string icon80Uri, string icon100Uri, string languages, Uri location)
    {
        Signature = signature;
        Name = name;
        DisplayName = displayName;
        Version = version;
        Author = author;
        Summary = summary;
        FirmwareMinimumVersion = firmwareMinimumVersion;
        TutorialLink = tutorialLink;
        ForumLink = forumLink;
        ChangelogLink = changelogLink;
        Category = category;
        Type = type;
        BannerImg = bannerImg;
        Icon80Uri = icon80Uri;
        Icon100Uri = icon100Uri;
        Languages = languages;
        Location = location;
    }

    private QpkgPackage()
    {
    }


    public static QpkgPackage Create(string packagePath, IList<string> otherFiles, QpkgRepositorySource source, QpkgRepositoryConfiguration configuration, Uri siteRoot)
    {
        using var fileStream = File.OpenRead(packagePath);
        var config = source.GetRawControl(fileStream);
        config.TryGetValue("QPKG_VER_LONG", out var version);

        var packageVersion = new Version(version ?? config["QPKG_VER"]);

        var packageName = config["QPKG_NAME"];
        var conf = GetConfigurationFile(packageName, otherFiles);
        return new QpkgPackage
        {
            Author = config["QPKG_AUTHOR"],
            Name = packageName,
            DisplayName = config["QPKG_DISPLAY_NAME"],
            Summary = config["QPKG_SUMMARY"],
            Version = packageVersion,
            PublishedDate = File.GetLastWriteTime(packagePath),
            Signature = File.ReadAllText(packagePath + ".codesigning"),
            Location = GetUri(siteRoot, packagePath),
            Icon80Uri = GetIcon(packageName, otherFiles, configuration, 80, siteRoot),
            Icon100Uri = GetIcon(packageName, otherFiles, configuration, 100, siteRoot),
            Category = conf.GetValueOrDefault("category") ?? string.Empty,
            Type = conf.GetValueOrDefault("type") ?? string.Empty,
            Languages = conf.GetValueOrDefault("language") ?? string.Empty,
            TutorialLink = conf.GetValueOrDefault("tutoriallink") ?? string.Empty,
            ChangelogLink = conf.GetValueOrDefault("changelog") ?? string.Empty,
            ForumLink = conf.GetValueOrDefault("forumlink") ?? string.Empty,
            SnapshotUri = conf.GetValueOrDefault("snapshot"),
            BannerImg = conf.GetValueOrDefault("bannerimg") ?? string.Empty,
            FirmwareMinimumVersion = config["QTS_MINI_VERSION"],
        };
    }

    private static Uri GetUri(Uri websiteRoot, string location)
    {
        return new Uri(websiteRoot, location);
    }

    private static string GetIcon(string packageName, IEnumerable<string> otherFiles, QpkgRepositoryConfiguration configuration, int size, Uri siteRoot)
    {
        var iconName = $"{packageName}_{size}";
        var iconLocalPath = otherFiles.FirstOrDefault(x => x.Contains(iconName));

        if (iconLocalPath is null || !File.Exists(iconLocalPath))
            return string.Empty;
        return GetUri(siteRoot, iconLocalPath).AbsoluteUri;
    }

    private static IDictionary<string, string> GetConfigurationFile(string packageName, IEnumerable<string> otherFiles)
    {
        try
        {
            var configFile = otherFiles.FirstOrDefault(x => x.EndsWith($"{packageName}.conf"));
            if (configFile is null || !File.Exists(configFile))
                return new Dictionary<string, string>();

            return ParseConfiguration(File.ReadAllText(configFile));
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }
    }

    private static IDictionary<string, string> ParseConfiguration(string config, char separator = '=')
    {
        var configuration = new Dictionary<string, string>();
        foreach (var line in config.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
        {
            var elements = line.Split(separator);
            if (elements.Length != 2)
                continue;
            var key = elements[0].ToLower();
            configuration[key] = elements[1].Trim(" \t\n\r\0\x0B\"".ToCharArray());
        }
        return configuration;
    }
}