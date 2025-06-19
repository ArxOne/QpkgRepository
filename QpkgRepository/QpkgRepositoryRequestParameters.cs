namespace ArxOne.Qnap;

using System;
using System.Collections.Immutable;

public class QpkgRepositoryRequestParameters
{
    public IImmutableDictionary<string, string> Values { get; init; } = ImmutableDictionary<string, string>.Empty;
    public required Uri RequestUri { get; init; }
    public Uri SiteRoot => new Uri(RequestUri, "/");
}
