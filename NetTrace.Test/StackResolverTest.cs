using System.Diagnostics.Tracing;

namespace NetTrace.Test;

public class StackResolverTest
{
    private static readonly EventMetadata Metadata = new(1, null, "Test", 1, "Sample", null,
        EventKeywords.None, 0, EventLevel.Informational, null, [], null);

    [Test]
    public void ManagedSymbolsAndStackCachesAreProcessScoped()
    {
        StackResolver resolver = new();
        resolver.AddMethodSymbolInfo(10, Method("First", 0x1000, 0x100));
        resolver.AddMethodSymbolInfo(20, Method("Second", 0x1000, 0x100));
        resolver.AddStackAddresses(1, [0x1010]);
        resolver.AddStackAddresses(2, [0x1010]);
        var first = CreateEvent(10, 1);
        var second = CreateEvent(20, 1);
        var firstDuplicate = CreateEvent(10, 2);
        var secondDuplicate = CreateEvent(20, 2);

        resolver.ResolveEventStackTraces([first, second, firstDuplicate, secondDuplicate]);

        Assert.Multiple(() =>
        {
            Assert.That(FrameNames(first), Is.EqualTo(new[] { "First" }));
            Assert.That(FrameNames(second), Is.EqualTo(new[] { "Second" }));
            Assert.That(firstDuplicate.StackTrace, Is.SameAs(first.StackTrace));
            Assert.That(secondDuplicate.StackTrace, Is.SameAs(second.StackTrace));
            Assert.That(second.StackTrace, Is.Not.SameAs(first.StackTrace));
            Assert.That(second.StackTrace.Index, Is.Not.EqualTo(first.StackTrace.Index));
            Assert.That(first.StackTrace.Addresses, Is.EqualTo(new ulong[] { 0x1010 }));
            Assert.That(second.StackTrace.Addresses, Is.EqualTo(new ulong[] { 0x1010 }));
        });
    }

    [Test]
    public void NativeSymbolsAreProcessScoped()
    {
        StackResolver resolver = new();
        resolver.AddProcessMapping(10, 1, 0x1000, 0x2000, 0, "first.so", 0);
        resolver.AddProcessMapping(20, 2, 0x1000, 0x2000, 0, "second.so", 0);
        resolver.AddProcessSymbol(1, 0x1000, 0x10ff, "First");
        resolver.AddProcessSymbol(2, 0x1000, 0x10ff, "Second");
        resolver.AddStackAddresses(1, [0x1010]);
        var first = CreateEvent(10, 1);
        var second = CreateEvent(20, 1);

        resolver.ResolveEventStackTraces([first, second]);

        Assert.Multiple(() =>
        {
            Assert.That(FrameNames(first), Is.EqualTo(new[] { "First" }));
            Assert.That(FrameNames(second), Is.EqualTo(new[] { "Second" }));
            Assert.That(first.StackTrace.Frames[0].Namespace, Is.EqualTo("first.so"));
            Assert.That(second.StackTrace.Frames[0].Namespace, Is.EqualTo("second.so"));
            Assert.That(first.StackTrace.Index, Is.Not.EqualTo(second.StackTrace.Index));
        });
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void SharedMappingSymbolsBindToAllProcessesRegardlessOfArrivalOrder(int symbolPosition)
    {
        StackResolver resolver = new();
        for (int i = 0; i <= 2; i += 1)
        {
            if (i == symbolPosition)
            {
                resolver.AddProcessSymbol(7, 0x1000, 0x10ff, "Shared");
            }
            if (i < 2)
            {
                resolver.AddProcessMapping(10 + i, 7, 0x1000, 0x2000, 0, $"process-{i}.so", 0);
            }
        }
        resolver.AddStackAddresses(1, [0x1010]);
        var first = CreateEvent(10, 1);
        var second = CreateEvent(11, 1);

        resolver.ResolveEventStackTraces([first, second]);

        Assert.Multiple(() =>
        {
            Assert.That(FrameNames(first), Is.EqualTo(new[] { "Shared" }));
            Assert.That(FrameNames(second), Is.EqualTo(new[] { "Shared" }));
            Assert.That(first.StackTrace.Frames[0].Namespace, Is.EqualTo("process-0.so"));
            Assert.That(second.StackTrace.Frames[0].Namespace, Is.EqualTo("process-1.so"));
            Assert.That(first.StackTrace.Index, Is.Not.EqualTo(second.StackTrace.Index));
        });
    }

    [Test]
    public void SharedMappingRetainsManyProcessAssociations()
    {
        StackResolver resolver = new();
        resolver.AddProcessSymbol(7, 0x1000, 0x10ff, "Shared");
        List<Event> events = [];
        for (int i = 0; i < 64; i += 1)
        {
            resolver.AddProcessMapping(i, 7, 0x1000, 0x2000, 0, "shared.so", 0);
            events.Add(CreateEvent(i, 1));
        }
        resolver.AddStackAddresses(1, [0x1010]);

        resolver.ResolveEventStackTraces(events);

        Assert.Multiple(() =>
        {
            Assert.That(events.SelectMany(FrameNames), Is.All.EqualTo("Shared"));
            Assert.That(events.Select(evt => evt.StackTrace.Index).Distinct().Count(), Is.EqualTo(64));
        });
    }

    [Test]
    public void ResolvedIndexesAreUniqueAcrossProcessesAndAddressGroups()
    {
        StackResolver resolver = new();
        resolver.AddMethodSymbolInfo(10, Method("First", 0x1000, 0x100));
        resolver.AddMethodSymbolInfo(20, Method("Second", 0x1000, 0x100));
        resolver.AddStackAddresses(1, [0x1010]);
        resolver.AddStackAddresses(2, [0x1010]);
        resolver.AddStackAddresses(3, [0x1020]);
        List<Event> events = [CreateEvent(10, 1), CreateEvent(20, 1), CreateEvent(10, 3), CreateEvent(20, 3)];
        var duplicate = CreateEvent(10, 2);
        events.Add(duplicate);

        resolver.ResolveEventStackTraces(events);

        Assert.Multiple(() =>
        {
            Assert.That(events.Take(4).Select(evt => evt.StackTrace.Index).Distinct().Count(), Is.EqualTo(4));
            Assert.That(duplicate.StackTrace, Is.SameAs(events[0].StackTrace));
            Assert.That(events.Take(4).Select(evt => evt.StackTrace.Index), Is.All.GreaterThanOrEqualTo(0));
        });
    }

    [TestCase(-1)]
    [TestCase(99)]
    [TestCase(20)]
    public void MissingProcessSymbolsDoNotFallBackToOtherProcesses(long processId)
    {
        StackResolver resolver = new();
        resolver.AddMethodSymbolInfo(10, Method("Managed", 0x1000, 0x100));
        resolver.AddProcessMapping(10, 1, 0x2000, 0x3000, 0, "native.so", 0);
        resolver.AddProcessSymbol(1, 0x2000, 0x20ff, "Native");
        // Process 20 has some symbols, but none at either sampled address.
        resolver.AddMethodSymbolInfo(20, Method("Unrelated", 0x3000, 0x100));
        resolver.AddStackAddresses(1, [0x1010, 0x2010]);
        var evt = CreateEvent(processId, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.Multiple(() =>
        {
            Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "??" }));
            Assert.That(evt.StackTrace.Addresses, Is.EqualTo(new ulong[] { 0x1010, 0x2010 }));
        });
    }

    [Test]
    public void SymbolsWithoutAnyMappingRemainUnresolved()
    {
        StackResolver resolver = new();
        resolver.AddProcessSymbol(7, 0x1000, 0x10ff, "Unassociated");
        resolver.AddStackAddresses(1, [0x1010]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??" }));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SymbolSortUsesFull64BitAddresses(bool native)
    {
        StackResolver resolver = new();
        ulong[] starts = [0x1000, 0x1_0000_1000, 0x8000_0000_0000_1000, ulong.MaxValue - 0x1f];
        if (native)
        {
            resolver.AddProcessMapping(10, 1, 0, ulong.MaxValue, 0, "native.so", 0);
        }
        for (int i = starts.Length - 1; i >= 0; i -= 1)
        {
            if (native)
            {
                resolver.AddProcessSymbol(1, starts[i], starts[i] + 0xf, $"Method{i}");
            }
            else
            {
                resolver.AddMethodSymbolInfo(10, Method($"Method{i}", starts[i], 0x10));
            }
        }
        ulong[] addresses = starts.Select(address => address + 1).ToArray();
        resolver.AddStackAddresses(1, addresses);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.Multiple(() =>
        {
            Assert.That(FrameNames(evt), Is.EqualTo(new[] { "Method0", "Method1", "Method2", "Method3" }));
            Assert.That(evt.StackTrace.Addresses, Is.EqualTo(addresses));
        });
    }

    [Test]
    public void NativeRangesLargerThan4GbUseInclusiveBounds()
    {
        StackResolver resolver = new();
        const ulong start = 0x1000;
        const ulong end = 0x2_0000_2000;
        resolver.AddProcessMapping(10, 1, start, end, 0, "large.so", 0);
        resolver.AddProcessSymbol(1, start, end, "Large");
        resolver.AddStackAddresses(1, [start - 1, start, start + uint.MaxValue + 1, end, end + 1]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "Large", "Large", "Large", "??" }));
    }

    [Test]
    public void NativeRangeIncludesUlongMaxValueWithoutOverflow()
    {
        StackResolver resolver = new();
        const ulong start = ulong.MaxValue - 0x1_0000_0000;
        resolver.AddProcessMapping(10, 1, start, ulong.MaxValue, 0, "max.so", 0);
        resolver.AddProcessSymbol(1, start, ulong.MaxValue, "Maximum");
        resolver.AddStackAddresses(1, [start - 1, start, ulong.MaxValue - 1, ulong.MaxValue]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "Maximum", "Maximum", "Maximum" }));
    }

    [Test]
    public void SingleAddressNativeRangeIncludesItsOnlyAddress()
    {
        StackResolver resolver = new();
        resolver.AddProcessMapping(10, 1, 0, ulong.MaxValue, 0, "single.so", 0);
        resolver.AddProcessSymbol(1, ulong.MaxValue, ulong.MaxValue, "Single");
        resolver.AddStackAddresses(1, [ulong.MaxValue - 1, ulong.MaxValue]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "Single" }));
    }

    [Test]
    public void ReversedNativeRangeIsIgnored()
    {
        StackResolver resolver = new();
        resolver.AddProcessMapping(10, 1, 0x1000, 0x2000, 0, "invalid.so", 0);
        resolver.AddProcessSymbol(1, 0x1100, 0x1000, "Invalid");
        resolver.AddStackAddresses(1, [0x1000, 0x1100, 0x1200]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "??", "??" }));
    }

    [Test]
    public void ManagedRangeHasExclusiveEnd()
    {
        StackResolver resolver = new();
        resolver.AddMethodSymbolInfo(10, Method("Managed", 0x1000, 0x10));
        resolver.AddStackAddresses(1, [0xfff, 0x1000, 0x100f, 0x1010, 0x1011]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "Managed", "Managed", "??", "??" }));
    }

    [Test]
    public void ManagedRangeNearUlongMaxValueDoesNotOverflow()
    {
        StackResolver resolver = new();
        const ulong start = ulong.MaxValue - 0x10;
        resolver.AddMethodSymbolInfo(10, Method("Managed", start, 0x20));
        resolver.AddStackAddresses(1, [0, start - 1, start, ulong.MaxValue]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "??", "Managed", "Managed" }));
    }

    [Test]
    public void ZeroSizeManagedMethodDoesNotResolveItsStart()
    {
        StackResolver resolver = new();
        resolver.AddMethodSymbolInfo(10, Method("Empty", 0x1000, 0));
        resolver.AddStackAddresses(1, [0x1000]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??" }));
    }

    [Test]
    public void ManagedAndNativeSymbolsCanResolveInTheSameProcess()
    {
        StackResolver resolver = new();
        resolver.AddMethodSymbolInfo(10, Method("Managed", 0x1000, 0x100));
        resolver.AddProcessMapping(10, 1, 0x2000, 0x3000, 0, "native.so", 0);
        resolver.AddProcessSymbol(1, 0x2000, 0x20ff, "Native");
        resolver.AddStackAddresses(1, [0x1010, 0x2010]);
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.That(FrameNames(evt), Is.EqualTo(new[] { "Managed", "Native" }));
    }

    [Test]
    public void OriginalInstructionPointersAreCopiedAndPreservedForUnresolvedFrames()
    {
        StackResolver resolver = new();
        ulong[] input = [0x1001, 0x2002];
        resolver.AddStackAddresses(1, input);
        input[0] = 0;
        var evt = CreateEvent(10, 1);

        resolver.ResolveEventStackTraces([evt]);

        Assert.Multiple(() =>
        {
            Assert.That(FrameNames(evt), Is.EqualTo(new[] { "??", "??" }));
            Assert.That(evt.StackTrace.Addresses, Is.EqualTo(new ulong[] { 0x1001, 0x2002 }));
        });
    }

    [Test]
    public void EmptyAndMissingStacksUseEmptyStackTrace()
    {
        StackResolver resolver = new();
        resolver.AddStackAddresses(1, []);
        var empty = CreateEvent(10, 1);
        var missing = CreateEvent(20, 2);

        resolver.ResolveEventStackTraces([empty, missing]);

        Assert.Multiple(() =>
        {
            Assert.That(empty.StackTrace, Is.SameAs(StackTrace.Empty));
            Assert.That(missing.StackTrace, Is.SameAs(StackTrace.Empty));
            Assert.That(StackTrace.Empty.Frames, Is.Empty);
            Assert.That(StackTrace.Empty.Addresses, Is.Empty);
            Assert.That(StackTrace.Empty.Index, Is.EqualTo(-1));
        });
    }

    [Test]
    public void ResolutionRebuildsSymbolsAndCachesWhenMoreMappingsArrive()
    {
        StackResolver resolver = new();
        resolver.AddStackAddresses(7, [0x1010]);
        resolver.AddProcessSymbol(1, 0x1000, 0x10ff, "Late");
        var first = CreateEvent(10, 7);
        var second = CreateEvent(20, 7);
        List<Event> events = [first, second];
        resolver.ResolveEventStackTraces(events);
        Assert.That(events.SelectMany(FrameNames), Is.All.EqualTo("??"));

        resolver.AddProcessMapping(10, 1, 0x1000, 0x2000, 0, "late.so", 0);
        resolver.ResolveEventStackTraces(events);
        Assert.That(FrameNames(first), Is.EqualTo(new[] { "Late" }));
        Assert.That(FrameNames(second), Is.EqualTo(new[] { "??" }));

        resolver.AddProcessMapping(20, 1, 0x1000, 0x2000, 0, "late.so", 0);
        resolver.ResolveEventStackTraces(events);
        Assert.Multiple(() =>
        {
            Assert.That(events.SelectMany(FrameNames), Is.All.EqualTo("Late"));
            Assert.That(first.StackTrace.Index, Is.Not.EqualTo(second.StackTrace.Index));
        });
    }

    private static MethodDescription Method(string name, ulong address, uint size) => new(name, "Managed", "", address, size);

    private static Event CreateEvent(long processId, int stackIndex) => new(0, 0, 0, 0, stackIndex, 0,
        Guid.Empty, Guid.Empty, new Dictionary<string, object>(), Metadata) { ProcessId = processId };

    private static string[] FrameNames(Event evt) => evt.StackTrace.Frames.Select(frame => frame.Name).ToArray();
}
