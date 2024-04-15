using System.Diagnostics;

namespace EventPipe.Test;

public class Tests
{
    [TestCaseSource(nameof(EnumerateTestCases))]
    public async Task Test1(string path)
    {
        for (int i = 0; i < 10; i += 1)
        {
            Stopwatch s = Stopwatch.StartNew();
            await using var f = File.OpenRead(path);
            EventPipeReader reader = new(f);
            var allocatedBytes1 = GC.GetTotalAllocatedBytes(precise: true);
            var trace = await reader.ReadFullTraceAsync();
            GC.Collect(GC.MaxGeneration);
            var allocatedBytes2 = GC.GetTotalAllocatedBytes(precise: true);
            var allocatedBytes3 = (allocatedBytes2 - allocatedBytes1) / 1_000_000f;
            float fileSize = f.Length / 1_000_000f;
            float ratio = 100 * allocatedBytes3 / fileSize;
            Console.WriteLine($"{s.ElapsedMilliseconds} ms, {trace.Events.Count} events, file size {fileSize:0.0} MB, {allocatedBytes3:0.0} MB allocated (+{ratio:0}%)");
        }
    }

    [Test]
    public async Task Test2()
    {
        string path = @"C:\Users\grego\Downloads\Rider.Backend.exe_20231209_205913.nettrace";
        MemoryStream s = new(File.ReadAllBytes(path));
        while (true)
        {
            s.Position = 0;
            EventPipeReader reader = new(s);
            await reader.ReadFullTraceAsync();
        }
    }

    private static IEnumerable<string> EnumerateTestCases()
    {
        return Directory.EnumerateFiles(@"C:\Users\grego\Downloads", "*.nettrace");
    }
}