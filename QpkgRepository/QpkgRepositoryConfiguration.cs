namespace ArxOne.Qnap;

using System.IO;

public record QpkgRepositoryConfiguration
{
    public string? CacheName { get; init; }

    /// <summary>
    /// Gets or sets the cache directory.
    /// </summary>
    /// <value>
    /// The cache directory.
    /// </value>
    private readonly string? _cacheDirectory;
    private readonly bool _cacheDirectorySet;

    public string? CacheDirectory
    {
        get => _cacheDirectorySet ? _cacheDirectory : GetDefaultCacheDirectory();
        init
        {
            _cacheDirectory = value;
            _cacheDirectorySet = true;
        }
    }

    /// <summary>
    /// Gets or sets the storage root.
    /// </summary>
    /// <value>
    /// The storage root.
    /// </value>
    public string StorageRoot { get; }

    public OnQpkgVersionFailed OnVersionFailed { get; }

    public QpkgRepositoryConfiguration(string storageRoot, OnQpkgVersionFailed onVersionFailed)
    {
        StorageRoot = storageRoot;
        OnVersionFailed = onVersionFailed;
    }

    private string GetDefaultCacheDirectory()
    {
        var defaultCacheDirectory = Path.Combine(Path.GetTempPath(), "qpkg-repository");
        if (CacheName is not null)
            defaultCacheDirectory = Path.Combine(defaultCacheDirectory, CacheName);
        return defaultCacheDirectory;
    }
}