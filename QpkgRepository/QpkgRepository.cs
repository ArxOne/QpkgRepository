using System.Xml.Linq;

namespace ArxOne.Qnap;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;
using Utility;

public class QpkgRepository
{
    private readonly QpkgRepositoryConfiguration _configuration;

    private readonly IReadOnlyList<QpkgRepositorySource> _sources;

    public QpkgRepository(QpkgRepositoryConfiguration qpkgRepositoryConfiguration, IEnumerable<QpkgRepositorySource> sources)
    {
        _configuration = qpkgRepositoryConfiguration;
        _sources = sources.ToImmutableList();
    }

    public void Reload()
    {
        foreach (var source in _sources)
            source.Cache = null;
    }

    public XDocument GetXml(Func<string, Version?>? onVersionFailed) => GetXml(null, onVersionFailed);

    public XDocument GetXml(string[]? model, Func<string, Version?>? onVersionFailed = null)
    {
        var platforms = model is { Length: > 0 } ? model.Select(x => x.Replace(" ", "+")).ToImmutableArray() : DefaultPlatforms;
        var packagesBysSource = LoadPackagesBySource(onVersionFailed);

        var latestPackages = packagesBysSource.GroupBy(x => x.Name).Select(g => g.MaxBy(p => p.Version)!);
        var itemElements = latestPackages.Select(p => CreateItemElement(p, platforms));

        var plugins = new XElement("plugins",
            new XElement("cachechk", DateTime.Now.ToString("yyyyMMddHHmm"))
        );
        plugins.Add(itemElements);
        return new XDocument(plugins);
    }

    private XElement CreateItemElement(QpkgPackage package, IEnumerable<string> platforms)
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
    
    private List<QpkgPackage> LoadPackagesBySource(Func<string, Version?>? onVersionFailed = null)
    {
        var packagesBySource = new List<QpkgPackage>(); 
        foreach (var source in _sources)
        {
            var files = Directory.GetFiles(source.SourceRelativeDirectory).ToList();
            packagesBySource.AddRange(LoadPackagesFromSource(files, source, onVersionFailed));
        }
        return packagesBySource;
    }

    private List<QpkgPackage> LoadPackagesFromSource(IList<string> files, QpkgRepositorySource source, Func<string, Version?>? onVersionFailed = null)
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
                    var loadPackagesFromSource = QpkgPackage.Create(filePath, files.Except(filePaths).ToList(), source, _configuration, () => onVersionFailed?.Invoke(filePath));
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
        "HS-251",
        "HS-251+",
        "HS-251D",
        "MiroKing",
        "Mustang-200",
        "QB-103",
        "QGD-1600",
        "QGD-1602",
        "QGD-3014",
        "SS-X53",
        "TNC-X51B",
        "TS-1232XU",
        "TS-1635AX",
        "TS-1685",
        "TS-231P2",
        "TS-231P3",
        "TS-251+",
        "TS-431P",
        "TS-431P2",
        "TS-431P3",
        "TS-451+",
        "TS-531P",
        "TS-673A",
        "TS-832X",
        "TS-EC1080 Pro",
        "TS-EC1280U",
        "TS-EC1280U R2",
        "TS-EC1280U-RP",
        "TS-EC1680U",
        "TS-EC1680U R2",
        "TS-EC1680U-RP",
        "TS-EC2480U",
        "TS-EC2480U R2",
        "TS-EC2480U-RP",
        "TS-EC880 Pro",
        "TS-EC880U",
        "TS-EC880U R2",
        "TS-EC880U-RP",
        "TS-i410X",
        "TS-KVM",
        "TS-NASARM",
        "TS-NASX86",
        "TS-X16",
        "TS-X28A",
        "TS-X31P2",
        "TS-X31P3",
        "TS-X31X",
        "TS-X31XU",
        "TS-X32",
        "TS-X32U",
        "TS-X33",
        "TS-X35",
        "TS-X35A",
        "TS-X35EU",
        "TS-X41",
        "TS-X51",
        "TS-X51+",
        "TS-X51A",
        "TS-X51AU",
        "TS-X51B",
        "TS-X51D",
        "TS-X51DU",
        "TS-X51U",
        "TS-X53",
        "TS-X53B",
        "TS-X53BU",
        "TS-X53D",
        "TS-X53E",
        "TS-X53II",
        "TS-X53U",
        "TS-X63",
        "TS-X63U",
        "TS-X64",
        "TS-X64U",
        "TS-X71",
        "TS-X71U",
        "TS-X72",
        "TS-X72U",
        "TS-X73",
        "TS-X73A",
        "TS-X73AU",
        "TS-X73U",
        "TS-X74",
        "TS-X75",
        "TS-X75U",
        "TS-X77",
        "TS-X77U",
        "TS-X79U",
        "TS-X80",
        "TS-X80U",
        "TS-X82",
        "TS-X82S",
        "TS-X82U",
        "TS-X83XU",
        "TS-X85",
        "TS-X85U",
        "TS-X87XU",
        "TS-X88",
        "TS-X88U",
        "TS-X89FU",
        "TS-X89U",
        "TS-X90",
        "TS-X90U",
        "TS-XA28A",
        "TS-XA51",
        "TS-XA73",
        "TS-XA82",
        "TS-XA83XU",
        "TVS-463",
        "TVS-663",
        "TVS-672X",
        "TVS-672XT",
        "TVS-863",
        "TVS-863+",
        "TVS-872X",
        "TVS-872XT",
        "TVS-882ST3"
    ];
}