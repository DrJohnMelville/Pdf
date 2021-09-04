using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public interface IParsingReader : IDisposable
    {
        long GlobalPosition { get; }
        long Position { get; }
        IPdfObjectParser RootObjectParser { get; }
        IIndirectObjectResolver IndirectResolver { get; }
        ParsingFileOwner Owner { get; }
        ValueTask<ReadResult> ReadAsync(CancellationToken token = default);

        void AdvanceTo(SequencePosition consumed);
        void AdvanceTo(SequencePosition consumed, SequencePosition examined);

        /// <summary>
        /// This method enables a very specific pattern that is common with parsing from the PipeReader.
        ///
        /// the pattern is do{}while(source.ShouldContinue(Method(await source.ReadAsync)));
        ///
        /// Method returns a pair (bool, SequencePosition).  Method can use out parameters for "real"
        /// return values.
        ///
        /// This pattern repeately reads the stream until method successfully parses, then it advances
        /// the reader to the given sequence position.
        /// </summary>
        bool ShouldContinue((bool Success, SequencePosition Position) result);

        PipeReader AsPipeReader();
        ValueTask AdvanceToPositionAsync(long targetPosition);
        IDecryptor Decryptor();
    }

    public class ParsingReader : CountingPipeReader, IParsingReader
    {
        private long lastSeek;
        public long GlobalPosition => lastSeek + Position;
        public IPdfObjectParser RootObjectParser => Owner.RootObjectParser;
        public IIndirectObjectResolver IndirectResolver => Owner.IndirectResolver;
        public IDecryptor Decryptor ()=> NullDecryptor.Instance;

        public ParsingFileOwner Owner { get; }

        public ParsingReader(
            ParsingFileOwner owner, PipeReader reader, long lastSeek) :
            base(reader)
        {
            this.lastSeek = lastSeek;
            Owner = owner;
        }

        public void Dispose()
        {
            Owner.ReturnReader(this);
            Complete();
        }

        /// <summary>
        /// This method enables a very specific pattern that is common with parsing from the PipeReader.
        ///
        /// the pattern is do{}while(source.ShouldContinue(Method(await source.ReadAsync)));
        ///
        /// Method returns a pair (bool, SequencePosition).  Method can use out parameters for "real"
        /// return values.
        ///
        /// This pattern repeately reads the stream until method successfully parses, then it advances
        /// the reader to the given sequence position.
        /// </summary>
        public bool ShouldContinue((bool Success, SequencePosition Position) result)
        {
            if (result.Success)
            {
                AdvanceTo(result.Position);
                return false;
            }

            MarkSequenceAsExamined();
            return true;
        }

        public PipeReader AsPipeReader() => this;

        public ValueTask AdvanceToPositionAsync(long targetPosition) =>
            BytesToAdvanceBy(targetPosition) switch
            {
                < 0 => throw new InvalidOperationException("Cannot rewind a pipe reader"),
                0 => new ValueTask(),
                _ => TryAdvanceFast(BytesToAdvanceBy(targetPosition))
            };

        private long BytesToAdvanceBy(long targetPosition) => targetPosition - Position;

        private ValueTask TryAdvanceFast(long delta)
        {
            if (TryRead(out var rr) && rr.Buffer.Length >= delta)
            {
                AdvanceTo(rr.Buffer.GetPosition(delta));
                return new ValueTask();
            }

            return SlowAdvanceToPositionAsync(delta);
        }

        private async ValueTask SlowAdvanceToPositionAsync(long delta)
        {
            while (true)
            {
                var ret = await ReadAsync();
                if (ret.Buffer.Length > delta)
                {
                    AdvanceTo(ret.Buffer.GetPosition(delta));
                    return;
                }

                if (ret.IsCompleted) return;
                AdvanceTo(ret.Buffer.Start, ret.Buffer.End);
            }
        }
    }
}