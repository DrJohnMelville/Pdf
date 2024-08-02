using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams
{
    public class MultiBufferStream2 : DefaultBaseStream, IMultiplexSource
    {
        private protected readonly MultiBuffer multiBuffer;
        private MultiBufferPosition position;

        internal MultiBufferStream2(MultiBuffer multiBuffer) : base(true, true, true)
        {
            this.multiBuffer = multiBuffer;
            position = multiBuffer.StartOfStream();
        }

        /// <summary>
        /// Create a MultDufferStream
        /// </summary>
        /// <param name="blockLength">The default block length when the stream creates blocks.</param>
        public MultiBufferStream2(int blockLength = 4096) : this(new MultiBuffer(blockLength))
        {
        }

        /// <summary>
        /// Create a multibufferstream that contains the given data
        /// </summary>
        /// <param name="firstBuffer"></param>
        public MultiBufferStream2(byte[] firstBuffer) : this(new MultiBuffer(firstBuffer))
        {
        }

        /// <inheritdoc />
        public override int Read(Span<byte> buffer) =>
            UpdatePosition(multiBuffer.Read(position, buffer));

        private int UpdatePosition(in MultiBufferPosition newPosition)
        {
            var ret = (int)(newPosition.StreamPosition - position.StreamPosition);
            position = newPosition;
            return ret;
        }

        /// <inheritdoc />
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            UpdatePosition(multiBuffer.Write(position, buffer));
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) =>
            Position = offset + this.SeekOriginLocation(origin);


        /// <inheritdoc />
        public override void SetLength(long value) => multiBuffer.SetLength(value);

        /// <inheritdoc />
        public override long Length => multiBuffer.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => position.StreamPosition;
            set => position = multiBuffer.FindPosition(value);
        }

        /// <summary>
        /// Create a reader that has its own unique position pointer into the buffer.
        /// </summary>
        public MultiBufferStream2 CreateReader() => new MultiBufferReader(multiBuffer);

        /// <inheritdoc />
        Stream IMultiplexSource.ReadFrom(long position)
        {
            var ret = CreateReader();
            ret.Seek(position, SeekOrigin.Begin);
            return ret;
        }


        private class MultiBufferReader : MultiBufferStream2
        {
            public MultiBufferReader(MultiBuffer multiBuffer) : base(multiBuffer)
            {
            }

            public override void Write(ReadOnlySpan<byte> buffer) =>
                throw new NotSupportedException();

            public override bool CanWrite => false;
        }
    }
}