using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing;
using Melville.Pdf.LowLevel.Parsing.NameParsing;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public static class TestParser
    {
        public static  Task<PdfObject> ParseTo(this string s) => ParseTo(s.AsExtendedAsciiBytes());

        public static Task<PdfObject> ParseTo(this byte[] bytes)
        {
            var root = new PdfCompositeObjectParser();
            return root.ParseAsync(new ParsingSource(new OneCharAtAtimeStream(bytes), root));
        }
    }
    
    public class OneCharAtAtimeStream: Stream
    {
        private byte[] data;

        public OneCharAtAtimeStream(byte[] data)
        {
            this.data = data;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Position >= data.Length) return 0;
            buffer[offset] = data[Position++];
            return 1;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.FromResult(Read(buffer, offset, count));
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
        {
            if (Position >= data.Length) return new ValueTask<int>(0);
            buffer.Span[0] = data[Position++];
            return new ValueTask<int>(1);
        }


        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => throw new System.NotSupportedException();

        public override long Length => throw new System.NotSupportedException();

        public override long Position { get; set; }
    }
}