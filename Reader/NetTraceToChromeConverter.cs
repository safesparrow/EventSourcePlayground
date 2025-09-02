using Microsoft.Diagnostics.Tracing;

public class NetTraceToChromeConverter
{
    private readonly string _inputPath;
    private readonly ChromeTraceWriter _writer;

    public NetTraceToChromeConverter(){}

    public void Convert(string inputPath)
    {
        using var source = new EventPipeEventSource(inputPath);
        // setup the callbacks
        source.Clr.All += Print;
        source.Kernel.All += Print;
        source.Dynamic.All += Print;
        source.AllEvents += Print;

        // iterate over the file, calling the callbacks.
        source.Process();
        var l = new FileInfo(inputPath).Length;
        var eCount = _idx;
        Console.WriteLine($"{inputPath} has {eCount} events, {l}b, that's {(double)l/(double)eCount}b/event");
    }

    private static int _idx = 0;
    private void Print(TraceEvent obj)
    {
        var bytes = obj.EventData();
        //if (obj.ProviderName.ToLower().Contains("rundown"))
        {
        Console.WriteLine($"[{_idx++}] {obj.EventName}: {obj.OpcodeName} {obj.ProviderName} {obj.PayloadNames.Length} payloads, {obj.PayloadByName("Message")}, {bytes.Length} bytes");
        var eventData = obj.EventData();
        }
    }
}
