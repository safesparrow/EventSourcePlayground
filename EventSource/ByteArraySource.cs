using System;
using System.Diagnostics.Tracing;

namespace EventSourceExample
{
    /// <summary>
    /// A custom EventSource designed to send a byte array as a payload.
    /// This uses unsafe code to directly write the binary data.
    /// </summary>
    public sealed class ByteArrayEventSource : EventSource
    {
        /// <summary>
        /// Provides a singleton instance for logging.
        /// </summary>
        public static readonly ByteArrayEventSource Log = new ByteArrayEventSource();

        // Private constructor to enforce the singleton pattern.
        // The name provided here is the "Provider Name" you will look for in tools like PerfView.
        private ByteArrayEventSource() : base("ByteArraySource") { }

        /// <summary>
        /// Sends a byte array as the payload for event ID 1.
        /// </summary>
        /// <remarks>
        /// This method is now decorated with [Event(1)], which generates a manifest.
        /// The manifest tells trace parsers that the payload for event 1 is a single
        /// binary blob (byte array). The parameter name 'data' will also be recorded,
        /// allowing access via methods like PayloadByName("data").
        /// We still use WriteEventCore for high performance, but now within a manifest-
        /// generating method.
        /// </remarks>
        /// <param name="data">The byte array payload to send.</param>
        [Event(1, Level = EventLevel.Informational)]
        public unsafe void Payload(byte[] data)
        {
            if (!IsEnabled()) return;
            if (data == null) { WriteEvent(1, (object?)null); return; }

            fixed (byte* p = data)
            {
                var ed = new EventSource.EventData();
                ed.DataPointer = (IntPtr)p;
                ed.Size = data.Length;
                WriteEventCore(1, 1, &ed);
            }
        }
    }

    /// <summary>
    /// A simple console application to demonstrate sending an event with a byte array.
    /// </summary>
    public class ByteProgram
    {
        public static void Main2(string[] args)
        {
            Console.WriteLine("EventSource Byte Array Demo");
            Console.WriteLine("---------------------------");

            // You can use tools like PerfView or dotnet-trace to capture these events.
            // In PerfView, collect events and look for the provider "MyCompany-ByteArrayEventSource".
            Console.WriteLine("Use a trace collection tool to listen for events from 'MyCompany-ByteArrayEventSource'.");
            
            // Define an event ID and a sample payload.
            const int myEventId = 1;
            byte[] payload = { 42 };

            Console.WriteLine($"\nSending event with ID {myEventId} and a payload of {payload.Length} bytes.");

            while (true)
            {
                Thread.Sleep(10);
                // Use the singleton instance of our EventSource to send the event.
                ByteArrayEventSource.Log.Payload(payload);
            }

            Console.WriteLine("Event has been sent.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}