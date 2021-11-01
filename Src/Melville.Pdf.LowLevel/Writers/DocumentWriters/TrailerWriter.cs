using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

public static class TrailerWriter
{
    private static readonly byte[] trailerTag =
        {116, 114, 97, 105, 108, 101, 114, 10}; // trailer\n
    private static readonly byte[] startXrefTag = 
        {10, 115, 116, 97, 114, 116, 120, 114, 101, 102, 10}; //\nstartxref\n
    private static readonly byte[] eofTag = {10, 37, 37, 69, 79, 70}; //\n%%EOF 
    public static async Task WriteTrailerWithDictionary(
        PipeWriter target, PdfDictionary dictionary, long xRefStart)
    {
        target.WriteBytes(trailerTag);
        await dictionary.Visit(new PdfObjectWriter(target));
        await WriteTerminalStartXrefAndEof(target, xRefStart);
    }

    public static ValueTask<FlushResult> WriteTerminalStartXrefAndEof(PipeWriter target, long xRefStart)
    {
        target.WriteBytes(startXrefTag);
        IntegerWriter.Write(target, xRefStart);
        target.WriteBytes(eofTag);
        return target.FlushAsync();
    }
}