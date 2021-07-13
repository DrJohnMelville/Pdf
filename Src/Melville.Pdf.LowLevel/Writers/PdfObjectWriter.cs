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
            var mem = target.GetSpan(5);
            item.TokenValue.AsSpan().CopyTo(mem);
            target.Advance(item.TokenValue.Length);
            return target.FlushAsync();
        }

        public override ValueTask<FlushResult> Visit(PdfString item) => 
            StringWriter.Write(target, item);

        public override ValueTask<FlushResult> Visit(PdfInteger item) =>
            IntegerWriter.Write(target, item.IntValue);

    }
}