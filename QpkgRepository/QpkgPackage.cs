﻿namespace ArxOne.Qnap;

using Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

[DebuggerDisplay($"{{{nameof(Name)}}} / {{{nameof(Architecture)}}}")]
public class QpkgPackage
{
    private const string DefaultLanguages = "English";
    private const string DefaultCategory = "More";

    [JsonPropertyName("signature")] public string Signature { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("displayName")] public string DisplayName { get; set; }

    [JsonPropertyName("literalVersion")] public string LiteralVersion { get; set; }

    [JsonPropertyName("version")] public Version Version { get; set; }

    [JsonPropertyName("author")] public string Author { get; set; }

    [JsonPropertyName("summary")] public string Summary { get; set; }

    [JsonPropertyName("firmwareMinimumVersion")] public string FirmwareMinimumVersion { get; set; }

    [JsonPropertyName("tutorialLink")] public string TutorialLink { get; set; }

    [JsonPropertyName("forumLink")] public string ForumLink { get; set; }

    [JsonPropertyName("changelogLink")] public string ChangelogLink { get; set; }

    [JsonPropertyName("category")] public string Category { get; set; }

    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("bannerImg")] public string BannerImg { get; set; }

    [JsonPropertyName("icon80Uri")] public string Icon80Uri { get; set; }

    [JsonPropertyName("icon100Uri")] public string Icon100Uri { get; set; }

    [JsonPropertyName("languages")] public string Languages { get; set; }

    [JsonPropertyName("publishedDate")] public DateTime PublishedDate { get; set; }

    [JsonPropertyName("localPath")] public string LocalPath { get; set; }

    [JsonPropertyName("location")] public Uri Location { get; set; }

    [JsonPropertyName("architecture")] public QpkgArchitecture Architecture { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonPropertyName("snapshot_uri")]
    public string? SnapshotUri { get; set; }

    public QpkgArchitecture[] Architectures => Architecture switch
    {
        QpkgArchitecture.All => [QpkgArchitecture.Arm64, QpkgArchitecture.X86_64, QpkgArchitecture.Arm32],
        _ => [Architecture]
    };

    private QpkgPackage(QpkgRepositoryConfiguration repositoryConfiguration, string packagePath, string literalVersion,
        Version packageVersion, IDictionary<string, string> configuration, IList<string> otherFiles)
    {
        var packageName = configuration.GetValueOrDefault("QPKG_NAME");
        IDictionary<string, string> conf = GetConfigurationFile(packageName, otherFiles);
        LocalPath = packagePath;
        Author = configuration.GetValueOrDefault("QPKG_AUTHOR");
        Name = packageName;
        DisplayName = configuration.GetValueOrDefault("QPKG_DISPLAY_NAME");
        Summary = configuration.GetValueOrDefault("QPKG_SUMMARY");
        LiteralVersion = literalVersion;
        Version = packageVersion;
        Architecture = GetArchitecture(packagePath);
        PublishedDate = File.GetLastWriteTime(packagePath);
        Signature = File.ReadAllText(packagePath + ".codesigning").Trim();
        Location = GetUri(packagePath, repositoryConfiguration);
        Icon80Uri = GetIcon(packageName, 80, repositoryConfiguration, otherFiles);
        Icon100Uri = GetIcon(packageName, 100, repositoryConfiguration, otherFiles);
        Category = conf.GetValueOrDefault("category", DefaultCategory);
        Type = conf.GetValueOrDefault("type");
        Languages = conf.GetValueOrDefault("language", DefaultLanguages);
        TutorialLink = conf.GetValueOrDefault("tutoriallink");
        ChangelogLink = conf.GetValueOrDefault("changelog");
        ForumLink = conf.GetValueOrDefault("forumlink");
        SnapshotUri = conf.GetValueOrDefault("snapshot");
        BannerImg = conf.GetValueOrDefault("bannerimg");
        FirmwareMinimumVersion = configuration.GetValueOrDefault("QTS_MINI_VERSION");
    }

    private static QpkgArchitecture GetArchitecture(string packagePath)
    {
        var fileName = Path.GetFileName(packagePath);
        return GetQpkgArchitecture(fileName, null) ?? QpkgArchitecture.All;
    }

    public static QpkgArchitecture? GetQpkgArchitecture(string value, bool? is64Bit)
    {
        if (value.Contains("x86_64", StringComparison.OrdinalIgnoreCase))
            return QpkgArchitecture.X86_64;
        if (value.Contains("x86", StringComparison.OrdinalIgnoreCase))
            return is64Bit == true ? QpkgArchitecture.X86_64 : QpkgArchitecture.X86;
        if (value.Contains("arm_64", StringComparison.OrdinalIgnoreCase))
            return QpkgArchitecture.Arm64;
        if (value.Contains("arm", StringComparison.OrdinalIgnoreCase))
            return is64Bit == true ? QpkgArchitecture.Arm64 : QpkgArchitecture.Arm32;
        return null;
    }

    public static QpkgPackage? Create(string packagePath, IList<string> otherFiles, QpkgRepositorySource source, QpkgRepositoryConfiguration configuration,
        Func<Version?>? onVersionFailed = null)
    {
        using var fileStream = File.OpenRead(packagePath);
        var config = source.GetRawControl(fileStream);
        var (literalVersion, packageVersion) = GetPackageVersion(config, onVersionFailed);
        return packageVersion is null ? null : new QpkgPackage(configuration, packagePath, literalVersion, packageVersion, config, otherFiles);
    }

    private static (string LiteralVersion, Version? Version) GetPackageVersion(IDictionary<string, string> config, Func<Version?>? onVersionFailed)
    {
        var literalVersion = config["QPKG_VER"];
        try
        {
            config.TryGetValue("QPKG_VER_LONG", out var version);
            return (literalVersion, new Version(version ?? literalVersion));
        }
        catch (FormatException)
        {
            return (literalVersion, onVersionFailed?.Invoke());
        }
    }

    public QpkgPackage()
    {
        Signature = string.Empty;
        Name = string.Empty;
        DisplayName = string.Empty;
        Version = new Version();
        Author = string.Empty;
        Summary = string.Empty;
        FirmwareMinimumVersion = string.Empty;
        TutorialLink = string.Empty;
        ForumLink = string.Empty;
        ChangelogLink = string.Empty;
        Category = DefaultCategory;
        LiteralVersion = string.Empty;
        Type = string.Empty;
        BannerImg = string.Empty;
        Icon80Uri = string.Empty;
        Icon100Uri = string.Empty;
        Languages = DefaultLanguages;
        LocalPath = string.Empty;
        Location = new Uri(string.Empty);
    }

    private static Uri GetUri(string location, QpkgRepositoryConfiguration configuration)
    {
        var storageRootLength = configuration.StorageRoot.Length + 1;

        return new Uri(configuration.SiteRoot, location[storageRootLength..]);
    }

    private static string GetIcon(string packageName, int size, QpkgRepositoryConfiguration configuration, IEnumerable<string> otherFiles)
    {
        var iconName = $"{packageName}_{size}";
        var iconLocalPath = otherFiles.FirstOrDefault(x => x.Contains(iconName));

        if (iconLocalPath is null || !File.Exists(iconLocalPath))
            return string.Empty;
        var uri = GetUri(iconLocalPath, configuration);
        return uri.AbsoluteUri;
    }

    private static Dictionary<string, string> GetConfigurationFile(string packageName, IEnumerable<string> otherFiles)
    {
        try
        {
            var configFile = otherFiles.FirstOrDefault(x => x.EndsWith($"{packageName}.conf"));
            if (configFile is null || !File.Exists(configFile))
                return [];
            return ParseConfiguration(File.ReadAllText(configFile));
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static Dictionary<string, string> ParseConfiguration(string config, char separator = '=')
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