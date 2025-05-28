using System.Diagnostics.Tracing;
using NetTrace;

namespace NetTrace.Test;

public class NetTraceReaderTest
{
    private static readonly byte[] HeaderBytes = Convert.FromBase64String("TmV0dHJhY2UUAAAAIUZhc3RTZXJpYWxpemF0aW9uLjE=");
    private static readonly byte[] FooterBytes = [0x1]; // NullReference tag.

    [Test]
    public async Task TraceObjectTest()
    {
        var trace = await ReadFullTraceAsync("BQUBBAAAAAQAAAAFAAAAVHJhY2UG5wcMAAIAGgARAC8ACgBuAk8T5s1YAwAAgJaYAAAAAAAIAAAAxAoAAAwAAABAQg8ABg==");
        Assert.Multiple(() =>
        {
            Assert.That(trace.Metadata.Date, Is.EqualTo(new DateTime(2023, 12, 26, 17, 47, 10, 622)));
            Assert.That(trace.Metadata.QueryPerformanceCounterSyncTime, Is.EqualTo(3679946412879));
            Assert.That(trace.Metadata.QueryPerformanceCounterFrequency, Is.EqualTo(10000000));
            Assert.That(trace.Metadata.PointerSize, Is.EqualTo(8));
            Assert.That(trace.Metadata.ProcessId, Is.EqualTo(2756));
            Assert.That(trace.Metadata.NumberOfProcessors, Is.EqualTo(12));
            Assert.That(trace.Metadata.CpuSamplingRate, Is.EqualTo(1000000));
        });
    }

    [Test]
    public async Task MetadataAndEventBlockTest()
    {
        var trace = await ReadFullTraceAsync("BQUBAgAAAAIAAAANAAAATWV0YWRhdGFCbG9jawZqAQAAAAAAFAABACtLjC4YzQUAK0uMLhjNBQDG/////w8A/////w+OFKuWsfSCo/MCvgIBAAAAUwB5AHMAdABlAG0ALgBUAGgAcgBlAGEAZABpAG4AZwAuAFQAYQBzAGsAcwAuAFQAcABsAEUAdgBlAG4AdABTAG8AdQByAGMAZQAAAAoAAABUAGEAcwBrAFcAYQBpAHQAQgBlAGcAaQBuAAAAAwAAAADwAAADAAAABAAAAAUAAAAJAAAATwByAGkAZwBpAG4AYQB0AGkAbgBnAFQAYQBzAGsAUwBjAGgAZQBkAHUAbABlAHIASQBEAAAACQAAAE8AcgBpAGcAaQBuAGEAdABpAG4AZwBUAGEAcwBrAEkARAAAAAkAAABUAGEAcwBrAEkARAAAAAkAAABCAGUAaABhAHYAaQBvAHIAAAAJAAAAQwBvAG4AdABpAG4AdQBlAFcAaQB0AGgAVABhAHMAawBJAEQAAAABAAAAAQkGBQUBAgAAAAIAAAAKAAAARXZlbnRCbG9jawZXAAAAAAAAFAABACtLjC4YzQUAboSOLhjNBQDPAQCCFP////8PghQBq5ax9IKj8wIUAQAAAAAAAAAEAAAAAgAAAAUAAAAIAsPyCAEAAAAAAAAABQAAAAIAAAADAAAABg==");
        Assert.Multiple(() =>
        {
            Assert.That(trace.Events, Has.Count.EqualTo(2));

            Assert.That(trace.Events[0].Metadata.MetadataId, Is.EqualTo(1));
            Assert.That(trace.Events[0].Metadata.ProviderName, Is.EqualTo("System.Threading.Tasks.TplEventSource"));
            Assert.That(trace.Events[0].Metadata.EventId, Is.EqualTo(10));
            Assert.That(trace.Events[0].Metadata.EventName, Is.EqualTo("TaskWaitBegin"));
            Assert.That(trace.Events[0].Metadata.Keywords, Is.EqualTo((EventKeywords)263882790666243));
            Assert.That(trace.Events[0].Metadata.Version, Is.EqualTo(3));
            Assert.That(trace.Events[0].Metadata.Level, Is.EqualTo(EventLevel.Informational));
            Assert.That(trace.Events[0].Metadata.Opcode, Is.EqualTo(EventOpcode.Send));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions, Has.Count.EqualTo(5));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[0].Name, Is.EqualTo("OriginatingTaskSchedulerID"));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[0].TypeCode, Is.EqualTo(TypeCode.Int32));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[1].Name, Is.EqualTo("OriginatingTaskID"));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[1].TypeCode, Is.EqualTo(TypeCode.Int32));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[2].Name, Is.EqualTo("TaskID"));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[2].TypeCode, Is.EqualTo(TypeCode.Int32));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[3].Name, Is.EqualTo("Behavior"));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[3].TypeCode, Is.EqualTo(TypeCode.Int32));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[4].Name, Is.EqualTo("ContinueWithTaskID"));
            Assert.That(trace.Events[0].Metadata.FieldDefinitions[4].TypeCode, Is.EqualTo(TypeCode.Int32));
            Assert.That(trace.Events[1].Metadata, Is.SameAs(trace.Events[0].Metadata));

            Assert.That(trace.Events[0].Index, Is.EqualTo(0));
            Assert.That(trace.Events[0].SequenceNumber, Is.EqualTo(1));
            Assert.That(trace.Events[0].CaptureThreadId, Is.EqualTo(2562));
            Assert.That(trace.Events[0].ThreadId, Is.EqualTo(2562));
            Assert.That(trace.Events[0].StackIndex, Is.EqualTo(1));
            // Assert.That(trace.Events[0].TimeStamp, Is.EqualTo(1632878627408683));
            Assert.That(trace.Events[0].ActivityId, Is.EqualTo(Guid.Empty));
            Assert.That(trace.Events[0].RelatedActivityId, Is.EqualTo(Guid.Empty));
            Assert.That(trace.Events[0].Payload, Has.Count.EqualTo(5));
            Assert.That(trace.Events[0].Payload["OriginatingTaskSchedulerID"], Is.EqualTo(1));
            Assert.That(trace.Events[0].Payload["OriginatingTaskID"], Is.EqualTo(0));
            Assert.That(trace.Events[0].Payload["TaskID"], Is.EqualTo(4));
            Assert.That(trace.Events[0].Payload["Behavior"], Is.EqualTo(2));
            Assert.That(trace.Events[0].Payload["ContinueWithTaskID"], Is.EqualTo(5));

            Assert.That(trace.Events[1].Index, Is.EqualTo(1));
            Assert.That(trace.Events[1].SequenceNumber, Is.EqualTo(2));
            Assert.That(trace.Events[1].CaptureThreadId, Is.EqualTo(2562));
            Assert.That(trace.Events[1].ThreadId, Is.EqualTo(2562));
            Assert.That(trace.Events[1].StackIndex, Is.EqualTo(2));
            // Assert.That(trace.Events[1].TimeStamp, Is.EqualTo(1632878627554414));
            Assert.That(trace.Events[1].ActivityId, Is.EqualTo(Guid.Empty));
            Assert.That(trace.Events[1].RelatedActivityId, Is.EqualTo(Guid.Empty));
            Assert.That(trace.Events[1].Payload, Has.Count.EqualTo(5));
            Assert.That(trace.Events[1].Payload["OriginatingTaskSchedulerID"], Is.EqualTo(1));
            Assert.That(trace.Events[1].Payload["OriginatingTaskID"], Is.EqualTo(0));
            Assert.That(trace.Events[1].Payload["TaskID"], Is.EqualTo(5));
            Assert.That(trace.Events[1].Payload["Behavior"], Is.EqualTo(2));
            Assert.That(trace.Events[1].Payload["ContinueWithTaskID"], Is.EqualTo(3));
        });
    }

    private static async Task<Trace> ReadFullTraceAsync(string objectsBase64)
    {
        byte[] objectsBytes = Convert.FromBase64String(objectsBase64);
        byte[] netTraceBytes = HeaderBytes.Concat(objectsBytes).Concat(FooterBytes).ToArray();
        using MemoryStream netTraceStream = new(netTraceBytes);
        NetTraceReader reader = new(netTraceStream);
        return await reader.ReadFullTraceAsync();
    }
}