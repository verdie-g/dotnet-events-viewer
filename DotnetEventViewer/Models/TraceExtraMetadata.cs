namespace DotnetEventViewer.Models;

public record TraceExtraMetadata(
    string Filename,
    string CommandLine,
    string OsFamily,
    Version RuntimeVersion,
    TimeSpan Duration);