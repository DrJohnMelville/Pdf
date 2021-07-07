using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.PdfStreamHolders
{
    public class ParsingSource
    {
        private readonly Stream source;
        public long Position { get; private set; }
        private PipeReader reader;

        public ParsingSource(Stream source)
        {
            this.source = source;
            CreatePipeReader();
        }

        [MemberNotNull(nameof(reader))]
        private void CreatePipeReader()
        {
            Position = source.Position;
            reader = PipeReader.Create(source, new StreamPipeReaderOptions(leaveOpen:true));
        }

        public ValueTask<ReadResult> ReadAsync(CancellationToken token = default)
        {
            return reader.ReadAsync(token);
        }

        public void AdvanceTo(in ReadOnlySequence<byte> sequence, SequencePosition consumed) =>
            AdvanceTo(sequence, consumed, consumed);
        public void AdvanceTo(
            in ReadOnlySequence<byte> sequence, SequencePosition consumed, SequencePosition examined)
        {
            Position += sequence.GetOffset(consumed);
            reader.AdvanceTo(consumed);
        }

        public void Seek(long newPosition)
        {
            reader.Complete();
            source.Seek(newPosition, SeekOrigin.Begin);
            Position = newPosition;
            CreatePipeReader();
        }
    }
}