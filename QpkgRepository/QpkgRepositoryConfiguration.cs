namespace ArxOne.Qnap;

public class QpkgRepositoryConfiguration
{

    /// <summary>
    /// Gets or sets the storage root.
    /// </summary>
    /// <value>
    /// The storage root.
    /// </value>
    public string StorageRoot { get; set; }

    /// <summary>
    /// Gets or sets the website root.
    /// </summary>
    /// <value>
    /// The website root.
    /// </value>
    public Uri WebsiteRoot { get; set; }

    public QpkgRepositoryConfiguration(string storageRoot, Uri websiteRoot)
    {
        StorageRoot = storageRoot;
        WebsiteRoot = websiteRoot;
    }
}