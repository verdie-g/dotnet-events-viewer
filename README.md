# .NET Events Viewer

This tool is a modern alternative to [PerfView](https://github.com/microsoft/perfview) to analyze nettrace files collected
from the .NET runtime's [EventPipe](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/eventpipe) or through the tool
[dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace). It has several benefits compared
to PerfView:
- **User Friendly**: even though I was using PerfView every week, I would always forget how to use it (e.g. how to filter
  GCAllocationTick events by large allocations again?). The interface of this project aims to be as intuitive as possible.
- **No Downloads**: the tool is compiled to WebAssembly and is available directly from your browser so you don't have
  yet-another-tool to download and keep up-to-date. Note that the analysed trace files never exit your browser.
- **Cross-Platform**: since it runs in the browser, this tool works on Windows, Mac, and Linux.

## Blog Posts

- Sept. 2024: [A Perfview Alternative in WebAssembly](https://techblog.criteo.com/a-perfview-alternative-in-webassembly-f6833820b699)
- Dec. 2024: [Investigate Thread Pool Starvation with .NET Events Viewer](https://techblog.criteo.com/investigate-thread-pool-starvation-with-net-events-viewer-1fa8453afd80)

## FAQ

### I'm getting a `File is too large to fit in memory` error when loading a nettrace

Unfortunately, WebAssembly currently only allows 32-bit addressing, so it means the decompressed file is limited to
4 GiB. Also, there is currently another issue, only 1 of the 4 GiB can be use [dotnet/runtime#101926](https://github.com/dotnet/runtime/issues/101926).

### Some events are named `Event XX` and don't have any payload

Nettrace files are expected to include the schemas for all their events, but this isn't the case for some providers
([dotnet/runtime#96365](https://github.com/dotnet/runtime/issues/96365)). As a workaround, you can manually define
these schemas in [this file](https://github.com/verdie-g/dotnet-events-viewer/blob/b4744a2f3a3edcacac89f149e746c9523c9447b0/EventPipe/KnownEvent.cs),
which also improves deserialization speed and results in a more compact memory representation.

### The feature X from PerfView is not available with this tool

This project aims to cover the most common use-cases according to my experience, i.e. find hot allocations path, find
large object allocations, find sync-over-async or other thread starving patterns... So it's possible that some PerfView
features are not available. Don't hesitate to open an issue if you feel like it should exist in this project.

### Why was a new nettrace parser written for this project?

PerfView's nettrace parser is actually a nettrace to etlx converter + an etlx parser. Also, its performance is not great
and it doesn't support async input which is mandatory to run in the browser.
