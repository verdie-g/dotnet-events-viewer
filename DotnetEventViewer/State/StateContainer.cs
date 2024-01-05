using DotnetEventViewer.Models;
using EventPipe;

namespace DotnetEventViewer.State;

internal class StateContainer
{
    private Trace? _trace;
    private TraceExtraMetadata? _traceExtraMetadata;

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

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}