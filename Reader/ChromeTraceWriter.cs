using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class ChromeTraceWriter
{
    private readonly List<object> _events = new List<object>();

    public void AddSpan(string category, int threadId, DateTime timestamp, DateTime endTimestamp, string name)
    {
        // Convert to microseconds since epoch for Chrome Trace
        var t0 = ToMicroseconds(timestamp);
        var t1 = ToMicroseconds(endTimestamp);
        _events.Add(new
        {
            name,
            cat = category,
            ph = "X",
            ts = t0,
            dur = Math.Max(0, t1 - t0),
            pid = 1,
            tid = threadId,
        });
    }

    public void AddEvent(string category, int threadId, DateTime timestamp, string name)
    {
        var t0 = ToMicroseconds(timestamp);
        _events.Add(new
        {
            name,
            cat = category,
            ph = "i",
            ts = t0,
            pid = 1,
            tid = threadId,
        });
    }

    public void WriteTo(string outputPath)
    {
        using var fs = File.Create(outputPath);
        using var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        writer.WritePropertyName("traceEvents");
        JsonSerializer.Serialize(writer, _events);
        writer.WriteEndObject();
        writer.Flush();
    }

    private static long ToMicroseconds(DateTime dt)
    {
        // Use UTC
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var diff = dt.ToUniversalTime() - epoch;
        return (long)(diff.TotalMilliseconds * 1000);
    }
}
