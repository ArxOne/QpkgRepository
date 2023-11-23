namespace ArxOne.Qnap;

using System.Collections.Immutable;
public class Package
{
    private IReadOnlyDictionary<string, string> Configuration { get; }

    public readonly QpkgRepositorySource Source;
    
    public readonly string Signature;

    public readonly string Name;

    public readonly string DisplayName;

    public readonly Version Version;

    public readonly string Author;

    public readonly string Summary;

    private readonly string _location;


    public Package(IDictionary<string, string> configuration, string location, string signature, QpkgRepositorySource source)
    {
        _location = location;
        Signature = signature;
        Source = source;
        Configuration = configuration.ToImmutableDictionary();
        Configuration.TryGetValue("QPKG_VER_LONG", out var version);
        Version = new Version(version ?? Configuration["QPKG_VER"]);
        Name = Configuration["QPKG_NAME"];
        DisplayName = Configuration["QPKG_DISPLAY_NAME"];
        Author = Configuration["QPKG_AUTHOR"];
        Summary = Configuration["QPKG_SUMMARY"];
    }

    public string GetUri(Uri websiteRoot)
    {
        var uri = new Uri(websiteRoot, _location);
        return uri.AbsoluteUri;
    }
}