using System.Diagnostics;
using System.Diagnostics.Tracing;
using EventSourceExample;

namespace EventSourceDemo
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var p = Process.Start("dotnet", ["trace", "collect", $"--process-id={Environment.ProcessId}", "--rundown", "false", "--providers=ByteArraySource:0xffffffffffffffff"]);
            ByteProgram.Main2([]);
            for(int i=0; i<1000; i++)
            {
                DemoEventSource.Log.AppStarted("Hello World!", 12);
                DemoEventSource.Log.DebugMessage("Got here");
                DemoEventSource.Log.DebugMessage("finishing startup");
                DemoEventSource.Log.RequestStart(3);
                DemoEventSource.Log.RequestStop(3);
                Console.WriteLine("Written");
            }
        }
    }
    

    [EventSource(Name = "Demo")]
   public class DemoEventSource : EventSource
    {
        public static DemoEventSource Log { get; } = new DemoEventSource();

        [Event(5, Level = EventLevel.Informational)]
        public void Test()
        {
            //WriteEventCore(5, 10, new EventData*);
        }
        
        [Event(1, Keywords = Keywords.Startup)]
        public void AppStarted(string message, int favoriteNumber) => WriteEvent(1, message, favoriteNumber);
        [Event(2, Keywords = Keywords.Requests)]
        public void RequestStart(int requestId) => WriteEvent(2, requestId);
        [Event(3, Keywords = Keywords.Requests)]
        public void RequestStop(int requestId) => WriteEvent(3, requestId);
        [Event(4, Keywords = Keywords.Startup, Level = EventLevel.Informational)]
        public void DebugMessage(string message) => WriteEvent(4, message);

        /// <param name="data">The byte array payload to send.</param>
        [Event(6, Level = EventLevel.Informational)]
        public unsafe void SendBytes(byte[] data)
        {
            fixed (byte* p = data)
            {
                var ed = new EventSource.EventData();
                ed.DataPointer = (IntPtr)p;
                ed.Size = data.Length;
                WriteEventCore(5, 1, &ed);
            }
        }

        public class Keywords
        {
            public const EventKeywords Startup = (EventKeywords)0x0001;
            public const EventKeywords Requests = (EventKeywords)0x0002;
        }
    }
}