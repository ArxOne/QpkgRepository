namespace ArxOne.Qnap;

using Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

public class QpkgPackage
{
    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("version")]
    public Version Version { get; set; }

    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("summary")]
    public string Summary { get; set; }

    [JsonPropertyName("firmwareMinimumVersion")]
    public string FirmwareMinimumVersion { get; set; }

    [JsonPropertyName("tutorialLink")]
    public string TutorialLink { get; set; }

    [JsonPropertyName("forumLink")]
    public string ForumLink { get; set; }

    [JsonPropertyName("changelogLink")]
    public string ChangelogLink { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("bannerImg")]
    public string BannerImg { get; set; }

    [JsonPropertyName("icon80Uri")]
    public string Icon80Uri { get; set; }

    [JsonPropertyName("icon100Uri")]
    public string Icon100Uri { get; set; }

    [JsonPropertyName("languages")]
    public string Languages { get; set; }

    [JsonPropertyName("publishedDate")]
    public DateTime PublishedDate { get; set; }

    [JsonPropertyName("localPath")]
    public string LocalPath { get; set; }

    [JsonPropertyName("location")]
    public Uri Location { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("snapshot_uri")]
    public string? SnapshotUri { get; set; }

    public QpkgPackage(string packagePath, IList<string> otherFiles, QpkgRepositorySource source, QpkgRepositoryConfiguration configuration, Uri siteRoot)
    {
        using var fileStream = File.OpenRead(packagePath);
        var config = source.GetRawControl(fileStream);
        config.TryGetValue("QPKG_VER_LONG", out var version);

        var packageVersion = new Version(version ?? config["QPKG_VER"]);

        var packageName = config["QPKG_NAME"];
        var conf = GetConfigurationFile(packageName, otherFiles);
        LocalPath = packagePath;
        Author = config["QPKG_AUTHOR"];
        Name = packageName;
        DisplayName = config["QPKG_DISPLAY_NAME"];
        Summary = config["QPKG_SUMMARY"];
        Version = packageVersion;
        PublishedDate = File.GetLastWriteTime(packagePath);
        Signature = File.ReadAllText(packagePath + ".codesigning");
        Location = GetUri(siteRoot, packagePath, configuration);
        Icon80Uri = GetIcon(packageName, 80, siteRoot, configuration, otherFiles);
        Icon100Uri = GetIcon(packageName, 100, siteRoot, configuration, otherFiles);
        Category = conf.GetValueOrDefault("category") ?? string.Empty;
        Type = conf.GetValueOrDefault("type") ?? string.Empty;
        Languages = conf.GetValueOrDefault("language") ?? "English";
        TutorialLink = conf.GetValueOrDefault("tutoriallink") ?? string.Empty;
        ChangelogLink = conf.GetValueOrDefault("changelog") ?? string.Empty;
        ForumLink = conf.GetValueOrDefault("forumlink") ?? string.Empty;
        SnapshotUri = conf.GetValueOrDefault("snapshot");
        BannerImg = conf.GetValueOrDefault("bannerimg") ?? string.Empty;
        FirmwareMinimumVersion = config["QTS_MINI_VERSION"];
    }

    public QpkgPackage()
    {
    }

    private static Uri GetUri(Uri siteRoot, string location, QpkgRepositoryConfiguration configuration)
    {
        var storageRootLength = configuration.StorageRoot.Length + 1;

        return new Uri(siteRoot, location[storageRootLength..]);
    }

    private static string GetIcon(string packageName, int size, Uri siteRoot, QpkgRepositoryConfiguration configuration, IEnumerable<string> otherFiles)
    {
        var iconName = $"{packageName}_{size}";
        var iconLocalPath = otherFiles.FirstOrDefault(x => x.Contains(iconName));

        if (iconLocalPath is null || !File.Exists(iconLocalPath))
            return string.Empty;
        var uri = GetUri(siteRoot, iconLocalPath, configuration);
        return uri.AbsoluteUri;
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