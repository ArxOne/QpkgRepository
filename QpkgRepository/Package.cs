namespace ArxOne.Qnap;

using System.Collections.Immutable;
public class Package
{
    private IReadOnlyDictionary<string, string> Configuration { get; }
    public string Location { get; set; }
    public string Signature { get; set; }

    public string Name => Configuration["QPKG_NAME"];
    public string DisplayName => Configuration["QPKG_DISPLAY_NAME"];
    public string Version => Configuration["QPKG_VER"];
    public string NumberVersion => Configuration["QPKG_VER"].Trim("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
    public string Author => Configuration["QPKG_AUTHOR"];
    public string Summary => Configuration["QPKG_SUMMARY"];

    public Package(IDictionary<string, string> configuration, string location, string signature)
    {
        Location = location;
        Signature = signature;
        Configuration = configuration.ToImmutableDictionary();
    }

    public string? GetIconPath(int size, string storageRoot, Uri websiteRoot)
    {
        var iconName = $"{Name}_{size}.gif";
        var iconPath = Path.Combine(Path.GetDirectoryName(Location) ?? string.Empty, iconName);
        var iconLocalPath = Path.Combine(storageRoot, iconPath);

        return !File.Exists(iconLocalPath) ? null : new Uri(websiteRoot, iconPath).AbsoluteUri; 
    }
    public string GetUri(Uri websiteRoot)
    {
        var uri = new Uri(websiteRoot, Location[1..]);
        return uri.AbsoluteUri;
    }
}   