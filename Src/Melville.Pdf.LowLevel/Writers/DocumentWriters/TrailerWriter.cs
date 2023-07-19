using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

internal static class TrailerWriter
{
    private static ReadOnlySpan<byte> TrailerTag => "trailer\n"u8;
    private static ReadOnlySpan<byte> StartXrefTag => "\nstartxref\n"u8;
    private static ReadOnlySpan<byte>  EofTag => "\n%%EOF"u8;
    public static async Task WriteTrailerWithDictionaryAsync(
        PipeWriter target, PdfValueDictionary dictionary, long xRefStart)
    {
        target.WriteBytes(TrailerTag);
        await dictionary.Visit(new PdfObjectWriter(target)).CA();
        await WriteTerminalStartXrefAndEofAsync(target, xRefStart).CA();
    }

    public static ValueTask<FlushResult> WriteTerminalStartXrefAndEofAsync(PipeWriter target, long xRefStart)
    {
        target.WriteBytes(StartXrefTag);
        IntegerWriter.Write(target, xRefStart);
        target.WriteBytes(EofTag);
        return target.FlushAsync();
    }
}