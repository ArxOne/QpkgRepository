namespace ArxOne.Qnap;


public class QpkgRepositorySource
{

    /// <summary>
    /// The source relative directory
    /// </summary>
    public readonly string SourceRelativeDirectory;

    /// <summary>
    /// The tutorial link
    /// </summary>
    public readonly string TutorialLink;

    /// <summary>
    /// The forum link
    /// </summary>
    public readonly string ForumLink;

    /// <summary>
    /// The changelog link
    /// </summary>
    public readonly string ChangelogLink;

    /// <summary>
    /// The category
    /// </summary>
    public readonly string Category;

    /// <summary>
    /// The type
    /// </summary>
    public readonly string Type;

    /// <summary>
    /// The banner img
    /// </summary>
    public readonly string BannerImg;

    /// <summary>
    /// The firmware minimum version
    /// </summary>
    public readonly string FirmwareMinimumVersion;

    /// <summary>
    /// The path of the icon with a size of 80 pixels
    /// </summary>
    public readonly string Icon80Uri;

    /// <summary>
    /// The path of the icon with a size of 100 pixels
    /// </summary>
    public readonly string Icon100Uri;

    /// <summary>
    /// The languages
    /// </summary>
    public readonly string Languages;

    /// <summary>
    /// The snapshot URI
    /// </summary>
    public readonly string? SnapshotUri;

    /// <summary>
    /// The published date  
    /// </summary>
    public readonly DateTime PublishedDate;

    /// <summary>
    /// Gets or sets the get raw control.
    /// </summary>
    /// <value>
    /// The get raw control.
    /// </value>
    public Func<Stream, IDictionary<string, string>> GetRawControl { get; }

    public QpkgRepositorySource(string sourceRelativeDirectory, string type, Func<Stream, IDictionary<string, string>> getRawControl,
        DateTime? publishedDate = null, string languages = "English, Français", string icon80Uri = "", string icon100Uri = "", string? snapshotUri = null,
        string category = "More", string tutorialLink = "", string changelogLink = "", string forumLink = "", string bannerImg = "",
        string firmwareMinimumVersion = "4.3.3")
    {
        SourceRelativeDirectory = sourceRelativeDirectory;
        TutorialLink = tutorialLink;
        ChangelogLink = changelogLink;
        Type = type;
        Category = category;
        PublishedDate = publishedDate ?? DateTime.Now;
        GetRawControl = getRawControl;
        SnapshotUri = snapshotUri;
        Languages = languages;
        FirmwareMinimumVersion = firmwareMinimumVersion;
        ForumLink = forumLink;
        BannerImg = bannerImg;
        Icon100Uri = icon100Uri;
        Icon80Uri = icon80Uri;
    }
}
