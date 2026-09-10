using System.Collections.Immutable;

namespace ArxOne.Qnap;

using System;
using System.Collections.Generic;
using System.IO;

public class QpkgRepositorySource
{
    public string? SourceID { get; }

    /// <summary>
    /// The source relative directory
    /// </summary>
    public string SourceRelativeDirectory { get; }

    /// <summary>
    /// Gets or sets the get raw control.
    /// </summary>
    /// <value>
    /// The get raw control.
    /// </value>
    public Func<Stream, ImmutableDictionary<string, string>> GetRawControl { get; }

    internal QpkgRepositoryCache? Cache { get; set; }

    public QpkgRepositorySource(string sourceRelativeDirectory, Func<Stream, ImmutableDictionary<string, string>> getRawControl, string? sourceID = null)
    {
        SourceRelativeDirectory = sourceRelativeDirectory;
        GetRawControl = getRawControl;
        SourceID = sourceID;
    }
}