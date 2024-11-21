namespace ArxOne.Qnap;

using System.Collections.Immutable;

public class QpkgRepositoryRequestParameters
{
    public IImmutableDictionary<string, string> Values { get; init; } = ImmutableDictionary<string, string>.Empty;
}
