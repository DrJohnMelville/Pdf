using System.Buffers;
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

        public override ValueTask<FlushResult> Visit(PdfTokenValues item) => 
            TokenValueWriter.Write(target, item);
        public override ValueTask<FlushResult> Visit(PdfString item) => 
            StringWriter.Write(target, item);
        public override ValueTask<FlushResult> Visit(PdfInteger item) =>
            IntegerWriter.Write(target, item.IntValue);
        public override ValueTask<FlushResult> Visit(PdfDouble item) =>
            DoubleWriter.Write(target, item.DoubleValue);
        public override ValueTask<FlushResult> Visit(PdfIndirectObject item) =>
            IndirectObjectWriter.Write(target, item, this);
        public override ValueTask<FlushResult> Visit(PdfIndirectReference item) =>
            IndirectObjectWriter.Write(target, item);
        public override ValueTask<FlushResult> Visit(PdfName item) =>
            NameWriter.Write(target, item);
        public override ValueTask<FlushResult> Visit(PdfArray item) => 
            ArrayWriter.Write(target, this, item.RawItems);

        public override ValueTask<FlushResult> Visit(PdfDictionary item) =>
            DictionaryWriter.Write(target, this, item.RawItems);
    }
}