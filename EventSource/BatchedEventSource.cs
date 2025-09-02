using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EventSourceExample;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct FrameHeader
{
    [FieldOffset(0)]
    public UInt16 EventId; // 2
    [FieldOffset(2)]
    public long TimestampUtc; // 8
    [FieldOffset(10)]
    public UInt16 PayloadLength; // 2
}

public sealed class BufferedEventSender : IDisposable
{
    private static readonly int HeaderSize = Unsafe.SizeOf<FrameHeader>();
    private readonly byte[] _buffer;
    private int _pos;
    private readonly ArrayPool<byte> _pool = ArrayPool<byte>.Shared;
    private bool _disposed;
    private readonly Action<ReadOnlySpan<byte>> _send;
    public long DroppedFrames { get; private set; }

    public BufferedEventSender(int bufferSize, Action<ReadOnlySpan<byte>> send)
    {
        _buffer = _pool.Rent(bufferSize);
        _pos = 0;
        _send = send;
    }

    public void EnqueueStruct<T>(ushort eventId, in T value) where T : unmanaged
    {
        int payloadSize = Unsafe.SizeOf<T>();
        if (payloadSize > UInt16.MaxValue)
        {
            ++DroppedFrames;
            return;
        }
        int totalFrameSize = HeaderSize + payloadSize;

        if (_pos + totalFrameSize > _buffer.Length)
        {
            if (totalFrameSize > _buffer.Length)
            {
                ++DroppedFrames;
                return;
            }

            Flush();
        }

        var destSpan = new Span<byte>(_buffer, _pos, totalFrameSize);
        var header = new FrameHeader
        {
            EventId = eventId,
            TimestampUtc = Stopwatch.GetTimestamp(),
            PayloadLength = (UInt16)payloadSize
        };
        MemoryMarshal.Write(destSpan, in header);
        MemoryMarshal.Write(destSpan.Slice(HeaderSize), in value);
        _pos += totalFrameSize;
    }

    public void Flush()
    {
        if (_pos == 0) return;
        _send(_buffer.AsSpan(0, _pos));
        _pos = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            Flush();
        }
        catch
        {
        }

        _pool.Return(_buffer);
        _disposed = true;
    }
}

[EventSource(Name = SourceName)]
public sealed class BatchedEventSource : EventSource
{
    public const int BatchEventId = 1;
    public const string SourceName = "BatchedEventSource";

    [NonEvent]
    public void SendBatch(ReadOnlySpan<byte> span)
    {
        Console.WriteLine($"SendBatch length={span.Length}");
        if (!IsEnabled() || span.Length == 0)
        {
            return;
        }
        unsafe
        {
            fixed (byte* p = span)
            {
                var ed = new EventData
                {
                    DataPointer = (IntPtr)p,
                    Size = span.Length
                };
                WriteEventCore(BatchEventId, 1, &ed);
            }
        }
    }
}