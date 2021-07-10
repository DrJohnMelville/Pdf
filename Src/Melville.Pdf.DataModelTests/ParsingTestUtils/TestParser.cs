using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public static class TestParser
    {
        public static Task<PdfObject> ParseToPdfAsync(this string s) =>
            ParseToPdfAsync(AsParsingSource(s));

        public static Task<PdfObject> ParseToPdfAsync(this byte[] bytes) => 
            ParseToPdfAsync(AsParsingSource(bytes));

        public static Task<PdfObject> ParseToPdfAsync(this ParsingSource source) => 
            source.RootParser.ParseAsync(source);

        public static ParsingSource AsParsingSource(this string str) =>
            AsParsingSource((str + " /%This simulates an end tag\r\n").AsExtendedAsciiBytes());
        public static ParsingSource AsParsingSource(this byte[] bytes) => 
            new(new OneCharAtAtimeStream(bytes), new PdfCompositeObjectParser());
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
            return Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => data.Length + offset,
            };
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

        public override bool CanSeek => true;

        public override bool CanWrite => throw new System.NotSupportedException();

        public override long Length => data.Length;

        public override long Position { get; set; }
    }
}