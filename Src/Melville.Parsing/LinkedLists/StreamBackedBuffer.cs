using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;

namespace Melville.Parsing.LinkedLists
{
    internal partial class StreamBackedBuffer: LinkedList<StreamBackedBuffer>
    {
        private bool done;
        public static MultiBufferStream Create(Stream source, int bufferSize = 4096)
        {
            var buffer = new StreamBackedBuffer(source);
            var linkedList = buffer.With(bufferSize);
            buffer.nextByteToRead = buffer.StartPosition;
            return new MultiBufferStream(linkedList, false);
        }

        [FromConstructor] private readonly Stream source;
        private LinkedListPosition nextByteToRead = default;
        private SemaphoreSlim mutex = new(1, 1);

        public override long PrepareForRead(LinkedListPosition origin)
        {
            var desiredPosition = origin.GlobalPosition;

            mutex.Wait();
            try
            {
                while (true)
                {
                    var delta = nextByteToRead.GlobalPosition - desiredPosition;
                    if (done || delta > 0) return Math.Max(delta, 0);
                    (nextByteToRead, done) = LoadFrom(source, nextByteToRead);
                }
            }
            finally
            {
                mutex.Release();
            }
        }

        public override async ValueTask<long> PrepareForReadAsync(
            LinkedListPosition origin)
        {
            var desiredPosition = origin.GlobalPosition;

            await mutex.WaitAsync().CA();
            try
            {
                while (true)
                {
                    var delta = nextByteToRead.GlobalPosition - desiredPosition;
                    if (done || delta > 0) return Math.Max(delta, 0);
                    (nextByteToRead, done) = await LoadFromAsync(source, nextByteToRead).CA();
                }
            }
            finally
            {
                mutex.Release();
            }
        }

        public override LinkedListPosition FirstInvalidPosition() => nextByteToRead;

        public override bool DoneGrowing() => done;
    }
}