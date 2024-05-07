namespace DotnetEventsViewer.Models;

public record TraceExtraMetadata(
    string Filename,
    string CommandLine,
    string OsFamily,
    Version RuntimeVersion,
    TimeSpan Duration);