using Melville.Parsing.Streams.Bases;

namespace Melville.Parsing.Streams;

public class MultiBufferStream : DefaultBaseStream
    {
        private readonly MultiBuffer multiBuffer;
        private MultiBufferPosition position;

        private MultiBufferStream(MultiBuffer multiBuffer): base(true, true, true)
        {
            this.multiBuffer = multiBuffer;
            position = multiBuffer.StartOfStream();
        }

        public MultiBufferStream(int blockLength = 4096) : this(new MultiBuffer(blockLength))
        {

        }

        public MultiBufferStream(byte[] firstBuffer) : this(new MultiBuffer(firstBuffer))
        {
        }

        public override int Read(Span<byte> buffer) =>
            UpdatePosition(multiBuffer.Read(position, buffer));

        private int UpdatePosition(in MultiBufferPosition newPosition)
        {
            var ret = (int)(newPosition.StreamPosition - position.StreamPosition);
            position = newPosition;
            return ret;
        }
        
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            UpdatePosition(multiBuffer.Write(position, buffer));
        }

        public override long Seek(long offset, SeekOrigin origin) => 
            Position = offset + SeekOriginLocation(origin);

        private long SeekOriginLocation(SeekOrigin origin) => origin switch
        {
            SeekOrigin.Current => Position,
            SeekOrigin.End => Length,
            _ => 0
        };

        public override void SetLength(long value) => multiBuffer.SetLength(value);
        
        public override long Length => multiBuffer.Length;

        public override long Position
        {
            get => position.StreamPosition;
            set => position = multiBuffer.FindPosition(value);
        }

        public MultiBufferStream CreateReader() => new MultiBufferReader(multiBuffer);


        private class MultiBufferReader : MultiBufferStream
        {
            public MultiBufferReader(MultiBuffer multiBuffer) : base(multiBuffer)
            {
            }

            public override void Write(ReadOnlySpan<byte> buffer) =>
                throw new NotSupportedException();

            public override bool CanWrite => false;
        }

    }