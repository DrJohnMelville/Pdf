using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;

namespace Melville.Parsing.LinkedLists;

[FromConstructor]
internal partial class MakeStreamSeekableSource : StreamBackedBuffer<MakeStreamSeekableSource>
{
    public static IMultiplexSource Create(Stream source, int bufferSize = 4096)
    {
        var linkedList = new MakeStreamSeekableSource(source, false).With(bufferSize);
        return new MultiBufferStream(linkedList, true, false, true);
    }

   private SemaphoreSlim mutex = new(1, 1);

    public override long PrepareForRead(LinkedListPosition origin)
    {
        mutex.Wait();
        try
        {
            return base.PrepareForRead(origin);
        }
        finally
        {
            mutex.Release();
        }
    }

    public override async ValueTask<long> PrepareForReadAsync(LinkedListPosition origin)
    {
        await mutex.WaitAsync().CA();
        try
        {
            return await base.PrepareForReadAsync(origin).CA();
        }
        finally
        {
            mutex.Release();
        }
    }
}