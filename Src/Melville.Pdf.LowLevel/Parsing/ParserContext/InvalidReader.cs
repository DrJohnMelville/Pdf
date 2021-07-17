using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext
{
    public class InvalidReader : PipeReader
    {
        public static readonly InvalidReader Instance = new InvalidReader();

        private InvalidReader() { }

        [DoesNotReturn]
        private void Throw() => throw new InvalidOperationException("Cannot read from a disposed ParsingReader");
        public override void AdvanceTo(SequencePosition consumed) => Throw();
        public override void AdvanceTo(SequencePosition consumed, SequencePosition examined) => Throw();
        public override void CancelPendingRead() => Throw();
        public override void Complete(Exception? exception = null) => Throw();

        public override ValueTask<ReadResult> ReadAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            Throw();
            return default;
        }
        public override bool TryRead(out ReadResult result)
        {
            Throw();
            result = default;
            return false;
        }
    }
}