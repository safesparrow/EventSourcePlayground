using System.Diagnostics;
using EventSourceDemo;
using EventSourceExample;

namespace Benchmarks;
using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

    public class Md5VsSha256
    {
        private const int N = 10000;
        private readonly byte[] data;

        private readonly SHA256 sha256 = SHA256.Create();
        private readonly MD5 md5 = MD5.Create();

        public Md5VsSha256()
        {
            data = new byte[N];
            new Random(42).NextBytes(data);
        }

        [Benchmark]
        public byte[] Sha256() => sha256.ComputeHash(data);

        [Benchmark]
        public byte[] Md5() => md5.ComputeHash(data);
    }

    public class SourceRuns
    {
        private Process _p;

        [BenchmarkDotNet.Attributes.GlobalSetup]
        public void Setup()
        {
            
            _p = Process.Start("/usr/local/share/dotnet/dotnet",
                ["trace", "collect", $"--process-id={Environment.ProcessId}", "--providers=BatchedEventSource:0xffffffffffffffff:", "-o", "/Users/janusz/RiderProjects/EventSource/Benchmarks/trace.nettrace"]);
            Thread.Sleep(2000);
            _b = new BatchedEventEmitter();
        }
        
        private byte[] _bytes = Enumerable.Range(0, 10000).Select(x => (byte)(x%240)).ToArray();
        private BatchedEventEmitter _b;

        struct A
        {
            public byte DpfmId { get; set; }
            public byte Operation { get; set; }
        }

        struct B
        {
            public byte Operation { get; set; }
        }
        
        [Benchmark]
        public void Test()
        {
            for (int i = 0; i < 20; i++)
            {
                _b.EnqueueStruct(7, new A{DpfmId = 7, Operation = 7});
                _b.EnqueueStruct(7, new A{DpfmId = 7, Operation = 7});
            }
            _b.Flush();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Thread.Sleep(2000);
            if (_p.HasExited)
            {
                throw new Exception("Process has exited");
            }

            _p.Kill();
            _p.WaitForExit();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            SourceRuns r = new SourceRuns();
            r.Setup();
            r.Test();
            r.Cleanup();
            
            //var summary = BenchmarkRunner.Run<SourceRuns>();
        }
    }