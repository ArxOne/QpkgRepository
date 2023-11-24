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
    
    public QpkgRepositoryConfiguration(string storageRoot)
    {
        StorageRoot = storageRoot;
    }
}