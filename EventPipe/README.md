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
EventPipeReader reader = new(session.EventStream);
Task<Trace> traceTask = reader.ReadFullTraceAsync();

await Task.Delay(5000);

await session.StopAsync(CancellationToken.None);
Trace trace = await traceTask;
```

## Read NetTrace file

Here the nettrace stream is read from a file.

```csharp
await using var nettraceStream = File.OpenRead(path);
EventPipeReader reader = new(nettraceStream);
Trace trace = await reader.ReadFullTraceAsync();
```