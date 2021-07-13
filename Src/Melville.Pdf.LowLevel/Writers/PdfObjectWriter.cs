using System;
using System.ComponentModel;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers
{
    public class PdfObjectWriter: RecursiveDescentVisitor<ValueTask<FlushResult>>
    {
        private readonly PipeWriter target;

        public PdfObjectWriter(PipeWriter target) : base(true)
        {
            this.target = target;
        }

        public override ValueTask<FlushResult> Visit(PdfTokenValues item)
        {
            var tokenLength = item.TokenValue.Length;
            var mem = target.GetSpan(tokenLength);
            item.TokenValue.AsSpan().CopyTo(mem);
            target.Advance(tokenLength);
            return target.FlushAsync();
        }

        public override ValueTask<FlushResult> Visit(PdfString item) => 
            StringWriter.Write(target, item);

        public override ValueTask<FlushResult> Visit(PdfInteger item) =>
            IntegerWriter.Write(target, item.IntValue);

        public override ValueTask<FlushResult> Visit(PdfDouble item) =>
            DoubleWriter.Write(target, item.DoubleValue);
    }
}