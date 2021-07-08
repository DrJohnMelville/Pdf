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
        public long Position { get; private set; }
        public IPdfObjectParser RootParser { get; }
        
        private readonly Stream source;
        private PipeReader reader;
        private ReadOnlySequence<byte> storedSequence;

        public ParsingSource(Stream source, IPdfObjectParser rootParser)
        {
            this.source = source;
            RootParser = rootParser;
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
            var valueTask = reader.ReadAsync(token);
            if (!valueTask.IsCompleted)
                return new ValueTask<ReadResult>(WaitForRead(valueTask));

            var res = valueTask.Result;
            StorePosition(res.Buffer);
            return new ValueTask<ReadResult>(res);
        }

        private async Task<ReadResult> WaitForRead(ValueTask<ReadResult> valueTask)
        {
            var ret = await valueTask;
            StorePosition(ret.Buffer);
            return ret;
        }

        private void StorePosition(ReadOnlySequence<byte> resBuffer)
        {
            storedSequence = resBuffer;
        }
        
        public void AdvanceTo(SequencePosition consumed) =>
            AdvanceTo(consumed, consumed);
        public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        {
            Position += storedSequence.GetOffset(consumed);
            reader.AdvanceTo(consumed, examined);
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