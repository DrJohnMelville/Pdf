using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.IStreamAdaptors;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.VisualBasic;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class FileTrailerLocater
{
    public static async Task<long> SearchAsync(ParsingFileOwner source, int fileTrailerSizeHint)
    {
        var blockLength = fileTrailerSizeHint + StartXRef.Length;
        for (long i = source.StreamLength - blockLength; i > -blockLength; i -= fileTrailerSizeHint)
        {
            if ((await SearchFromLocation(source, i, blockLength)) is >= 0 and var value)
                return value;
        }
        throw new PdfParseException("Could not find StartXref trailer");
    }

    private static async Task<long> SearchFromLocation(
        ParsingFileOwner source, long startingPosition, int blockLength)
    {
#warning IStream needs to become disposable, because many streams require destruction.
        var reader = source.SubsetIAsyncReader(Math.Max(startingPosition, 0));
        using var buffer = new ParsingBuffer(reader);
        await buffer.TryEnsureBytesAsync(blockLength);

        var position = buffer.Peek(blockLength).LastIndexOf(StartXRef);
        if (position < 0) return -1;

        return await FindXrefStartPosition(buffer, position);
    }

    private static async Task<long> FindXrefStartPosition(ParsingBuffer buffer, int position)
    {
        buffer.Advance(position + StartXRef.Length); // skip the startxref
        buffer.Advance(await buffer.EnsureHasChar(digits)); // skip any whitespace
        var end = await buffer.EnsureHasNonChar(digits); // make sure we read the whole number
        return long.Parse(buffer.Peek(end<0?buffer.CurrentLength:end));
    }

    private static readonly SearchValues<byte> digits = SearchValues.Create("0123456789"u8);
    private static ReadOnlySpan<byte> StartXRef => "startxref"u8; 

}