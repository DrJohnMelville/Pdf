using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

public static class XrefTableElementWriter
{
    private static readonly byte[] xrefHeader = {120, 114, 101, 102, 10}; // xref\n
    public static void WriteXrefTitleLine(PipeWriter target)
    {
        target.WriteBytes(xrefHeader);
    }
    public static void WriteTableHeader(PipeWriter target, long firstItem, int countOfEntries)
    {
        var span = target.GetSpan(25);
        var position = IntegerWriter.Write(span, firstItem);
        span[position++] = 32;
        position += IntegerWriter.Write(span.Slice(position), countOfEntries);
        span[position++] = 10;
        target.Advance(position);
    }

    public static void WriteTableEntry(
        PipeWriter target, long itemOffset, int generationNumber, bool isUsedEntry)
    {
        var buffer = target.GetSpan(20);
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer, itemOffset, 10);
        buffer[10] = 32;
        IntegerWriter.WriteFixedWidthPositiveNumber(buffer.Slice(11), 
            generationNumber,5);
        buffer[16] = 32;
        buffer[17] = isUsedEntry ? (byte) 'n' : (byte) 'f';
        buffer[18] = 13;
        buffer[19] = 10;
        target.Advance(20);
    }
}