# .NET Event Viewer

This tool is a modern alternative to [PerfView](https://github.com/microsoft/perfview) to analyze nettrace files collected
from the .NET runtime's [EventPipe](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/eventpipe) or through the tool
[dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace). It has several benefits compared
to PerfView:
- **User Friendly**: even though I was using PerfView every week, I would always forget how to use it (e.g. how to filter
  GCAllocationTick events by large allocations again?). The interface of this project aims to be as intuitive as possible.
- **No Downloads**: the tool is compiled to WebAssembly and is available directly from your browser so you don't have
  yet-another-tool to download and keep up-to-date. Note that the analysed trace files never exit your browser.
- **Cross-Platform**: since it runs in the browser, this tool works on Windows, Mac, and Linux.

## Limitations

When loading a nettrace file with this tool, the file will be fully decompressed to memory which can do a 5x on its size.
Meaning that a 200 MB nettrace file could fill 1 GB of memory. That could be an issue if you have a very large file or
a small amount of memory.

## FAQ

### The feature X from PerfView is not available with this tool

This project aims to cover the most common use-cases according to my experience, i.e. find hot allocations path, find
large object allocations, find sync-over-async or other thread starving patterns... So it's possible that some PerfView
features are not available. Don't hesitate to open an issue if you feel like it should exist in this project.

### Why was a new nettrace parser written for this project?

PerfView's nettrace parser is actually a nettrace to etlx converter + an etlx parser. Also, its performance is not great
and it doesn't support async input which is mandatory to run in the browser.
