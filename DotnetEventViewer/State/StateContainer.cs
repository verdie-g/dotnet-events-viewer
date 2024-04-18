using DotnetEventViewer.Models;
using DotnetEventViewer.Querying;
using EventPipe;

namespace DotnetEventViewer.State;

internal class StateContainer
{
    private Trace? _trace;
    private TraceExtraMetadata? _traceExtraMetadata;
    private Query? _query;

    public Trace? Trace
    {
        get => _trace;
        set
        {
            _trace = value;
            NotifyStateChanged();
        }
    }

    public TraceExtraMetadata? TraceExtraMetadata
    {
        get => _traceExtraMetadata;
        set
        {
            _traceExtraMetadata = value;
            NotifyStateChanged();
        }
    }

    public Query? Query
    {
        get => _query;
        set
        {
            _query = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}