using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;

namespace Melville.Parsing.LinkedLists
{
    internal partial class StreamBackedBuffer<T> : LinkedList<T> where T : LinkedList<T>
    {
        private bool done;

        public override LinkedList With(int blockSize)
        {
            var ret = base.With(blockSize);
            nextByteToRead = StartPosition;
            return ret;
        }

        [FromConstructor] private Stream source;
        [FromConstructor] private bool leaveOpen;
        private LinkedListPosition nextByteToRead = default;

        public override long PrepareForRead(LinkedListPosition origin)
        {
            var desiredPosition = origin.GlobalPosition;

            while (true)
            {
                var delta = nextByteToRead.GlobalPosition - desiredPosition;
                if (done || delta > 0) return Math.Max(delta, 0);
                (nextByteToRead, done) = LoadFrom(source, nextByteToRead);
            }
        }

        public override async ValueTask<long> PrepareForReadAsync(
            LinkedListPosition origin)
        {
            var desiredPosition = origin.GlobalPosition;

            while (true)
            {
                var delta = nextByteToRead.GlobalPosition - desiredPosition;
                if (done || delta > 0) return Math.Max(delta, 0);
                (nextByteToRead, done) = await LoadFromAsync(source, nextByteToRead).CA();
            }
        }

        public override LinkedListPosition FirstInvalidPosition() => nextByteToRead;

        public override bool DoneGrowing() => done;

        protected override void CleanUp()
        {
            base.CleanUp();
            if (!leaveOpen) source.Dispose();
            source = null!;
        }
    }
}