using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public partial class OneCharAtAtimeStream : Stream
    {
        [DelegateTo] private Stream source;

        public OneCharAtAtimeStream(byte[] source): this(new MemoryStream(source)){ { }
        }
        public OneCharAtAtimeStream(Stream source)
        {
            this.source = source;
        }
        public override IAsyncResult BeginRead(
            byte[] buffer, int offset, int count, AsyncCallback? callback, object? state) => 
            source.BeginRead(buffer, offset, 1, callback, state);
        public override int Read(byte[] buffer, int offset, int count) => source.Read(buffer, offset, 1);
        public override int Read(Span<byte> buffer) => source.Read(buffer[..1]);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => 
            source.ReadAsync(buffer, offset, 1, cancellationToken);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken) => 
            source.ReadAsync(buffer[..Math.Min(1, buffer.Length)], cancellationToken);
            
    }
}