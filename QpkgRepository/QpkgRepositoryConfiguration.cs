namespace ArxOne.Qnap;

using System;
using System.IO;

public class QpkgRepositoryConfiguration
{
    private bool _cacheDirectorySet;
    private string? _cacheDirectory;

    /// <summary>
    /// Gets or sets the cache directory.
    /// </summary>
    /// <value>
    /// The cache directory.
    /// </value>
    public string? CacheDirectory
    {
        get => _cacheDirectorySet ? _cacheDirectory : GetDefaultCacheDirectory();
        set
        {
            _cacheDirectory = value;
            _cacheDirectorySet = true;
        }
    }

    private readonly Func<Uri> _getSiteRoot;
    public Uri SiteRoot => _getSiteRoot();

    /// <summary>
    /// Gets or sets the storage root.
    /// </summary>
    /// <value>
    /// The storage root.
    /// </value>
    public string StorageRoot { get; }

    public OnQpkgVersionFailed OnVersionFailed { get; }

    public QpkgRepositoryConfiguration(Func<Uri> getSiteRoot, string storageRoot, OnQpkgVersionFailed onVersionFailed)
    {
        _getSiteRoot = getSiteRoot;
        StorageRoot = storageRoot;
        OnVersionFailed = onVersionFailed;
    }

    private static string GetDefaultCacheDirectory()
    {
        return Path.Combine(Path.GetTempPath(), "qpkg-repository");
    }
}