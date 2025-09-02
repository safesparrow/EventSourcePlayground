using EventSourceExample;

namespace Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    struct Op { public byte Code; }
    
    [Test]
    public void Test1()
    {
        var s = new BatchedEventSource();
        var t= new SelfTracer();
        
        var b = new BufferedEventSender(40, span => s.SendBatch(span.ToArray()));
        for (int i = 0; i < 1000; i++)
        {
            b.EnqueueStruct(1, new Op { Code = (byte)(i % 200) });
        }
        Assert.Pass();
    }
}