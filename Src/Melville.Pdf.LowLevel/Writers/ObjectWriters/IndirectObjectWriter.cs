using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal static class IndirectObjectWriter
{
    private static ReadOnlySpan<byte> ObjectLabel => " obj "u8;
    private static ReadOnlySpan<byte> ReferenceLabel => " R"u8;
    private static ReadOnlySpan<byte> EndObjLabel => " endobj\n"u8;

    public static ValueTask<FlushResult> WriteObjectReference(PipeWriter target, PdfIndirectObject item)
    {
        target.Advance(WriteObjectHeader(target.GetSpan(25), item, ReferenceLabel));
        return  target.FlushAsync();
    }

    private static int WriteObjectHeader(Span<byte> buffer, PdfIndirectObject item, ReadOnlySpan<byte> suffix)
    {
        var position = WriteObjectIdDigits(buffer, item);
        suffix.CopyTo(buffer.Slice(position));
        return position + suffix.Length;
    }

    public static async ValueTask<FlushResult> WriteObjectDefinition(
        PipeWriter target, PdfIndirectObject item,  ILowLevelVisitor<ValueTask<FlushResult>> innerWriter)
    {
        target.Advance(WriteObjectHeader(target.GetSpan(25), item, ObjectLabel)); 
        await target.FlushAsync().CA();
        await (await item.DirectValueAsync().CA()).Visit(innerWriter).CA();
        target.WriteBytes(EndObjLabel);
        return await target.FlushAsync().CA();
    }
        
    private static int WriteObjectIdDigits(Span<byte> buffer, PdfIndirectObject pdfIndirectObject)
    {
        int position = IntegerWriter.Write(buffer, pdfIndirectObject.ObjectNumber);
        buffer[position++] = 32; //space
        position += IntegerWriter.Write(buffer.Slice(position), pdfIndirectObject.GenerationNumber);
        return position;
    }
}