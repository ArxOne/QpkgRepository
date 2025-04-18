namespace ArxOne.Qnap;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Utility;

// ?model=TS-251&platform=X86_BAYTRAIL&fw_version=5.2.1&fw_number=2930&fw_date=20241025&lang=eng&64bit=1

public class QpkgRepository
{
    private readonly QpkgRepositoryConfiguration _configuration;

    private readonly IReadOnlyList<QpkgRepositorySource> _sources;
    private ImmutableArray<QpkgPackage>? _packages;
    private ImmutableArray<QpkgPackage> Packages => _packages ??= LoadPackagesBySource();

    public QpkgRepository(QpkgRepositoryConfiguration qpkgRepositoryConfiguration, IEnumerable<QpkgRepositorySource> sources)
    {
        _configuration = qpkgRepositoryConfiguration;
        _sources = sources.ToImmutableList();
    }

    public void Reload()
    {
        foreach (var source in _sources)
            source.Cache = null;
        if (_packages is not null)
            _packages = LoadPackagesBySource();
    }

    public XDocument GetXml(QpkgRepositoryRequestParameters parameters)
    {
        parameters.Values.TryGetValue("model", out var model);
        parameters.Values.TryGetValue("platform", out var platform);
        parameters.Values.TryGetValue("64bit", out var is64Bit);
        return GetXml(model is null ? [] : [model], platform, is64Bit == "1" || is64Bit == "true");
    }

    public XDocument GetXml(string[]? model, string? platforms, bool is64Bit)
    {
        var models = model is { Length: > 0 } ? model.Select(x => x.Replace(" ", "+")).ToImmutableArray() : DefaultPlatforms;
        var packagesBySource = Packages;
        var platformsArch = platforms is not null ? QpkgPackage.GetQpkgArchitecture(platforms, is64Bit) ?? QpkgArchitecture.Arm64 : QpkgArchitecture.Arm64;

        var groupBy = packagesBySource.Where(y => y.Architectures.Contains(platformsArch)).GroupBy(x => x.Name);
        var latestPackages = groupBy.Select(g => g.MaxBy(p => p.Version)!);

        var itemElements = latestPackages.Select(p => CreateItemElement(p, models));

        var plugins = new XElement("plugins",
            new XElement("cachechk", DateTime.Now.ToString("yyyyMMddHHmm"))
        );
        plugins.Add(itemElements);
        return new XDocument(plugins);
    }

    private static XElement CreateItemElement(QpkgPackage package, IEnumerable<string> platforms)
    {
        var itemElement = new XElement("item",
            new XElement("name", new XCData(package.DisplayName)),
            new XElement("internalName", package.Name),
            new XElement("description", new XCData(package.Summary)),
            new XElement("version", package.LiteralVersion),
            new XElement("maintainer", new XCData(package.Author)),
            new XElement("developer", new XCData(package.Author))
            );
        itemElement.Add(platforms.Select(p => CreatePlatformElement(package, p)));
        itemElement.Add(
            new XElement("category", package.Category),
            new XElement("type", package.Type),
            new XElement("changeLog", package.ChangelogLink),
            new XElement("publishedDate", package.PublishedDate.ToString("yyyy/MM/dd")),
            new XElement("language", package.Languages)
        );
        itemElement.AddXElementIfNotNullOrEmpty("icon100", package.Icon100Uri);
        itemElement.AddXElementIfNotNullOrEmpty("icon80", package.Icon80Uri);
        itemElement.AddXElementIfNotNullOrEmpty("snapshot", package.SnapshotUri);
        itemElement.AddXElementIfNotNullOrEmpty("forumLink", package.ForumLink);
        itemElement.AddXElementIfNotNullOrEmpty("bannerImg", package.BannerImg);
        itemElement.AddXElementIfNotNullOrEmpty("tutorialLink", package.TutorialLink);
        itemElement.Add(
            new XElement("fwVersion", package.FirmwareMinimumVersion)
        );
        return itemElement;
    }

    private ImmutableArray<QpkgPackage> LoadPackagesBySource()
    {
        return [.. _sources.SelectMany(s => LoadPackagesFromSource(Directory.GetFiles(s.SourceRelativeDirectory), s))];
    }

    private List<QpkgPackage> LoadPackagesFromSource(IReadOnlyCollection<string> files, QpkgRepositorySource source)
    {
        var packages = new List<QpkgPackage>();

        var repositoryCache = LoadPackageCache(source);
        var packageInformation = repositoryCache.Packages.ToDictionary(p => p.LocalPath);
        var removedPackageInformation = packageInformation.Keys.ToHashSet();
        var hasNew = false;


        var filePaths = files.Where(x => x.EndsWith(".qpkg")).ToList();
        foreach (var filePath in filePaths)
        {
            if (packageInformation.TryGetValue(filePath, out var package))
            {
                removedPackageInformation.Remove(filePath);
                packages.Add(package);

            }
            else
            {
                try
                {
                    var loadPackagesFromSource = QpkgPackage.Create(filePath, files.Except(filePaths).ToList(), source, _configuration, () => _configuration.OnVersionFailed?.Invoke(filePath));
                    if (loadPackagesFromSource is null)
                        continue;
                    packageInformation[filePath] = loadPackagesFromSource;
                    hasNew = true;
                    packages.Add(loadPackagesFromSource);
                }
                catch (FormatException) { }
                catch (FileNotFoundException) { }
            }
        }

        if (!hasNew && removedPackageInformation.Count <= 0)
            return packages;

        repositoryCache.Packages = packageInformation.Values.ToArray();
        SavePackageInformation(source, repositoryCache);
        return packages;
    }

    private string? GetCacheFilePath(QpkgRepositorySource source)
    {
        var cacheDirectory = _configuration.CacheDirectory;
        if (cacheDirectory is null)
            return null;
        var root = source.SourceRelativeDirectory.Trim('/').Replace('/', '-').Replace('\\', '-');
        if (!string.IsNullOrEmpty(root))
            cacheDirectory = Path.Combine(cacheDirectory, root);
        return cacheDirectory + ".json";
    }

    private QpkgRepositoryCache LoadPackageCache(QpkgRepositorySource source)
    {
        if (source.Cache is not null)
            return source.Cache;
        var cacheFilePath = GetCacheFilePath(source);
        if (cacheFilePath is null)
            return new QpkgRepositoryCache();
        if (!File.Exists(cacheFilePath))
            return new QpkgRepositoryCache();
        using var cacheReader = File.OpenRead(cacheFilePath);
        try
        {
            return JsonSerializer.Deserialize<QpkgRepositoryCache>(cacheReader) ?? new QpkgRepositoryCache();
        }
        catch
        {
            return new QpkgRepositoryCache();
        }
    }

    private void SavePackageInformation(QpkgRepositorySource source, QpkgRepositoryCache repositoryCache)
    {
        source.Cache = repositoryCache;
        var cacheFilePath = GetCacheFilePath(source);
        if (cacheFilePath is null)
            return;
        var cacheDirectory = Path.GetDirectoryName(cacheFilePath);
        if (cacheDirectory is not null && !Directory.Exists(cacheDirectory))
            Directory.CreateDirectory(cacheDirectory);
        using var cacheWriter = File.Create(cacheFilePath);
        JsonSerializer.Serialize(cacheWriter, repositoryCache);
    }

    private static XElement CreatePlatformElement(QpkgPackage package, string platform)
    {
        return new XElement("platform",
            new XElement("platformID", platform),
            new XElement("location", package.Location.AbsoluteUri),
            new XElement("signature", package.Signature)
        );
    }

    private static readonly IReadOnlyList<string> DefaultPlatforms =
    [
        "no-platform"
    ];
}