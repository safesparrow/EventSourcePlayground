using System;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using EventSourceExample;

public class SelfTracer : EventListener
{
    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (eventSource.Name == BatchedEventSource.SourceName)
        {
            EnableEvents(eventSource, EventLevel.LogAlways);
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (eventData.EventId != BatchedEventSource.BatchEventId || eventData.Payload.Count == 0)
            return;

        var buffer = (byte[])eventData.Payload[0];
        int pos = 0;
        while (pos + Marshal.SizeOf<FrameHeader>() <= buffer.Length)
        {
            var header = MemoryMarshal.Read<FrameHeader>(buffer.AsSpan(pos));
            Console.WriteLine($"EventId={header.EventId}, Timestamp={header.TimestampUtc}, PayloadLength={header.PayloadLength}");
            pos += Marshal.SizeOf<FrameHeader>() + header.PayloadLength;
        }
    }
}