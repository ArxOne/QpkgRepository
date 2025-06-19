namespace ArxOne.Qnap;

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

    private static string GetDefaultCacheDirectory()
    {
        return Path.Combine(Path.GetTempPath(), "qpkg-repository");
    }
}