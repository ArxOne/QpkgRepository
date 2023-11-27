using System.IO;

namespace ArxOne.Qnap;

public class QpkgRepositoryConfiguration
{
    private bool _cacheDirectorySet;
    private string? _cacheDirectory;
    public string? CacheDirectory
    {
        get { return _cacheDirectorySet ? _cacheDirectory : GetDefaultCacheDirectory(); }
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
    
    public QpkgRepositoryConfiguration(string storageRoot)
    {
        StorageRoot = storageRoot;
    }
    private string GetDefaultCacheDirectory()
    {
        return Path.Combine(Path.GetTempPath(), "qpkg-repository");
    }
}