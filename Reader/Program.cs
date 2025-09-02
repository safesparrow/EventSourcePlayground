// To run this code, you need to add the Microsoft.Diagnostics.Tracing.TraceEvent NuGet package:
// dotnet add package Microsoft.Diagnostics.Tracing.TraceEvent

using System;
using System.IO;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

/// <summary>
/// A minimal C# program to parse a .nettrace file and log specific events.
/// It listens for GC Start/Stop events and custom events from an EventSource named "demo".
/// </summary>
public class NetTraceReader
{
    public static void Main(string[] args)
    {
        // --- 1. Argument Validation ---
        // Ensure the user has provided a path to the .nettrace file.
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
        {
            Console.WriteLine("Error: Please provide the path to a .nettrace file as the first argument.");
            Console.WriteLine("Usage: dotnet run -- /path/to/your/trace.nettrace");
            return;
        }

        string tracePath = args[0];

        // Check if the specified file actually exists.
        if (!File.Exists(tracePath))
        {
            Console.WriteLine($"Error: The file '{tracePath}' was not found.");
            return;
        }

        Console.WriteLine($"Starting to process trace file: {tracePath}");

        var writer = new ChromeTraceWriter();
        var reader = new NetTraceToChromeConverter();
        reader.Convert(tracePath);
    }
}