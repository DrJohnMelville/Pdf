using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public partial class OneCharAtAtimeStream : Stream
    {
        private byte[] singleBuffer = new byte[1];
        [DelegateTo] private Stream source;
        public OneCharAtAtimeStream(byte[] source): this(new MemoryStream(source)){ { }
        }
        public OneCharAtAtimeStream(Stream source)
        {
            this.source = source;
        }
        public override IAsyncResult BeginRead(
            byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) =>
            throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var ret = source.Read(singleBuffer, 0, Math.Min(1, buffer.Length));
            if (ret > 0) buffer[offset] = singleBuffer[0];
            return ret;
        }

        public override int Read(Span<byte> buffer)
        {
            var ret = source.Read(singleBuffer, 0, Math.Min(1, buffer.Length));
            if (ret > 0) buffer[0] = singleBuffer[0];
            return ret;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var ret = await source.ReadAsync(singleBuffer, 0, Math.Min(buffer.Length, 1), cancellationToken);
            if (ret > 0) buffer[offset] = singleBuffer[0];
            return ret;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var ret = await source.ReadAsync(singleBuffer, 0, Math.Min(buffer.Length,1), cancellationToken);
            if (ret > 0) buffer.Span[0] = singleBuffer[0];
            return ret;
        }
    }
}