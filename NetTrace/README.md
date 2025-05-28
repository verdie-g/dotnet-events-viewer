# NetTrace/EventPipe Reader

## Read `EventPipeSession.EventStream`

In the following example, using the nuget `Microsoft.Diagnostics.NETCore.Client`
an event pipe session is started on a progress given its pid.
`EventPipeSession.EventStream` contains a stream of a nettrace file that could
be written on the disk but here all its events are deserialized in memory using
`EventPipeReader.ReadFullTraceAsync`.

```csharp
DiagnosticsClient client = new(pid);
EventPipeSession session = await client.StartEventPipeSessionAsync(
    [new EventPipeProvider("System.Threading.Tasks.TplEventSource", EventLevel.Verbose, 0x3)],
    true);
NetTraceReader reader = new(session.EventStream);
Task<Trace> traceTask = reader.ReadFullTraceAsync();

await Task.Delay(5000);

await session.StopAsync(CancellationToken.None);
Trace trace = await traceTask;
```

## Read NetTrace file

Here the nettrace stream is read from a file.

```csharp
await using var nettraceStream = File.OpenRead(path);
NetTraceReader reader = new(nettraceStream);
Trace trace = await reader.ReadFullTraceAsync();
```

### Full Example

This code reads deserialize a nettrace file from the disk and prints the allocation by type/code location.

```csharp
await using var nettraceStream = File.OpenRead("demo.nettrace");
NetTraceReader reader = new(nettraceStream);
Trace trace = await reader.ReadFullTraceAsync();

Dictionary<(string type, int stackId), (ulong totalSize, List<Event> evts)> allocationsByType = [];
foreach (var evt in trace.Events)
{
    if (evt.Metadata.EventId != 10 // GCAllocationTick
        || evt.Metadata.ProviderName != "Microsoft-Windows-DotNETRuntime")
    {
        continue;
    }

    string typeName = (string)evt.Payload["TypeName"];
    ulong objectSize = (ulong)evt.Payload["ObjectSize"];

    if (!allocationsByType.TryGetValue((typeName, evt.StackTrace.Index), out var x))
    {
        x = (0, []);
    }

    x.totalSize += objectSize;
    x.evts.Add(evt);
    allocationsByType[(typeName, evt.StackTrace.Index)] = x;
}

foreach (var kvp in allocationsByType.OrderByDescending(x => x.Value.totalSize))
{
    Console.Write("---------------------------------------------------------------- ");
    Console.Write($"{kvp.Key} ({kvp.Value.evts.Count} count, {kvp.Value.totalSize} bytes)");
    Console.WriteLine(" ----------------------------------------------------------------");
    for (int i = 0; i < Math.Min(kvp.Value.evts.Count, 5); i += 1)
    {
        var evt = kvp.Value.evts[i];
        Console.WriteLine($"----------- Event {evt.Index} ({evt.Payload["ObjectSize"]} B) -----------");
        for (int j = 0; j < 5; j += 1)
        {
            Console.WriteLine(evt.StackTrace.Frames[j]);
        }
    }
}
```