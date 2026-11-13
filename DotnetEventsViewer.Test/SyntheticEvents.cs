using System.Diagnostics.Tracing;
using System.Reflection;
using DotnetEventsViewer.Models;
using NetTrace;
using StackTrace = NetTrace.StackTrace;

namespace DotnetEventsViewer.Test;

// Keep reflection confined to fixture construction; assertions exercise the real app assembly.
internal static class SyntheticEvents
{
    private static readonly ConstructorInfo EventConstructor = typeof(Event).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic, null,
        [typeof(int), typeof(int), typeof(long), typeof(long), typeof(int), typeof(long),
            typeof(Guid), typeof(Guid), typeof(IReadOnlyDictionary<string, object>), typeof(EventMetadata)], null)!;
    private static readonly ConstructorInfo StackConstructor = typeof(StackTrace).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic, null,
        [typeof(int), typeof(MethodDescription[]), typeof(ulong[])], null)!;
    private static readonly ConstructorInfo MethodConstructor = typeof(MethodDescription).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic, null,
        [typeof(string), typeof(string), typeof(string), typeof(ulong), typeof(uint)], null)!;
    private static readonly MethodInfo StackSetter = typeof(Event).GetProperty(nameof(NetTrace.Event.StackTrace))!.GetSetMethod(true)!;
    private static readonly ConstructorInfo TraceConstructor = typeof(Trace).GetConstructor(
        BindingFlags.Instance | BindingFlags.NonPublic, null,
        [typeof(TraceMetadata), typeof(IReadOnlyList<EventMetadata>), typeof(IReadOnlyList<Event>)], null)!;

    internal static Trace Trace(params Event[] events) => (Trace)TraceConstructor.Invoke(
        [new TraceMetadata(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc), 0, 1_000_000_000, 8, 1, 1, 1000),
            events.Select(evt => evt.Metadata).Distinct().ToArray(), events]);

    internal static MethodDescription Method(string name, ulong address) =>
        (MethodDescription)MethodConstructor.Invoke([name, "Tests", "", address, (uint)1]);

    internal static StackTrace Stack(int index, params MethodDescription[] frames) =>
        (StackTrace)StackConstructor.Invoke([index, frames, frames.Select(frame => frame.Address).ToArray()]);

    internal static Event Event(int index, long timestamp = 0, long threadId = 1,
        StackTrace? stack = null, EventKey? key = null, IReadOnlyDictionary<string, object>? payload = null)
    {
        key ??= new EventKey("Tests", "Sample", 1);
        EventMetadata metadata = new(1, null, key.ProviderName, key.EventId, key.EventName, null,
            EventKeywords.None, 0, EventLevel.Informational, null, [], null);
        var evt = (Event)EventConstructor.Invoke([index, index, threadId, threadId, stack?.Index ?? -1,
            timestamp, Guid.Empty, Guid.Empty, payload ?? new Dictionary<string, object>(), metadata]);
        StackSetter.Invoke(evt, [stack ?? StackTrace.Empty]);
        return evt;
    }
}
