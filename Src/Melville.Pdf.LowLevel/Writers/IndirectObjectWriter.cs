using System;
using System.IO.Pipelines;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers
{
    public static class IndirectObjectWriter
    {
        private static byte[] ObjectLabel = {32, 111, 98, 106, 32}; // ' obj '
        private static byte[] ReferenceLabel = {32, 82}; // ' R'
        private static byte[] endObjLabel = {32, 101, 110, 100, 111, 98, 106}; //  endobj

        public static ValueTask<FlushResult> Write(PipeWriter target, PdfIndirectReference item)
        {
            target.Advance(WriteObjectHeader(target.GetSpan(25), item.Target, ReferenceLabel));
            return  target.FlushAsync();
        }

        private static int WriteObjectHeader(Span<byte> buffer, PdfIndirectObject item, byte[] suffix)
        {
            var position = WriteObjectIdDigits(buffer, item);
            suffix.AsSpan().CopyTo(buffer.Slice(position));
            return position + suffix.Length;
        }

        public static async ValueTask<FlushResult> Write(PipeWriter target, PdfIndirectObject item,
            ILowLevelVisitor<ValueTask<FlushResult>> innerWriter)
        {
            target.Advance(WriteObjectHeader(target.GetSpan(25), item, ObjectLabel)); 
            await target.FlushAsync();
            await item.Value.Visit(innerWriter);
            target.Advance(WriteEndObj(target.GetSpan(endObjLabel.Length)));
            return await target.FlushAsync();
        }

        private static int WriteEndObj(Span<byte> span)
        {
            endObjLabel.AsSpan().CopyTo(span);
            return endObjLabel.Length;
        }

        private static int WriteObjectIdDigits(Span<byte> buffer, PdfIndirectObject pdfIndirectObject)
        {
            int position = IntegerWriter.CopyNumberToBuffer(buffer, pdfIndirectObject.ObjectNumber);
            buffer[position++] = 32; //space
            position += IntegerWriter.CopyNumberToBuffer(buffer.Slice(position), pdfIndirectObject.GenerationNumber);
            return position;
        }
    }
}