using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

public static class IndirectObjectWriter
{
    private static byte[] ObjectLabel = {32, 111, 98, 106, 32}; // ' obj '
    private static byte[] ReferenceLabel = {32, 82}; // ' R'
    private static byte[] endObjLabel = {32, 101, 110, 100, 111, 98, 106, 10}; //  endobj

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
        await (await item.DirectValueAsync()).Visit(innerWriter);
        target.WriteBytes(endObjLabel);
        return await target.FlushAsync();
    }
        
    private static int WriteObjectIdDigits(Span<byte> buffer, PdfIndirectObject pdfIndirectObject)
    {
        int position = IntegerWriter.Write(buffer, pdfIndirectObject.ObjectNumber);
        buffer[position++] = 32; //space
        position += IntegerWriter.Write(buffer.Slice(position), pdfIndirectObject.GenerationNumber);
        return position;
    }
}