namespace ArxOne.Qnap;

using System;
using System.Collections.Generic;
using System.IO;

public class QpkgRepositorySource
{
    /// <summary>
    /// The source relative directory
    /// </summary>
    public readonly string SourceRelativeDirectory;

    /// <summary>
    /// Gets or sets the get raw control.
    /// </summary>
    /// <value>
    /// The get raw control.
    /// </value>
    public Func<Stream, IDictionary<string, string>> GetRawControl { get; }

    public QpkgRepositorySource(string sourceRelativeDirectory, Func<Stream, IDictionary<string, string>> getRawControl)
    {
        SourceRelativeDirectory = sourceRelativeDirectory;
        GetRawControl = getRawControl;
    }
}