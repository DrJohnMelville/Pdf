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
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging.Abstractions;
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
        Force.Using(source.SubsetIAsyncReader(Math.Max(startingPosition, 0)), out var reader);
        using var buffer = new ParsingBuffer(reader);
        await buffer.TryEnsureBytesAsync(blockLength);

        var position = buffer.Peek(blockLength).LastIndexOf(StartXRef);
        if (position < 0) return -1;

        return await FindXrefStartPosition(buffer, position);
    }

    private static async Task<long> FindXrefStartPosition(ParsingBuffer buffer, int position)
    {
        buffer.Advance(position + StartXRef.Length); // skip the startxref
        return await buffer.ReadTextLongAsync().CA() ??
            throw new PdfParseException("No xref position found.");
    }

    private static readonly SearchValues<byte> digits = SearchValues.Create("0123456789"u8);
    private static ReadOnlySpan<byte> StartXRef => "startxref"u8; 

}
internal static class FileParsingBufferExtensions
{
    extension(ParsingBuffer self) 
    { 
        public async ValueTask<long?> ReadTextLongAsync()
        {
            await self.SkipOverAsync(CharacterClassifier.WhitespaceSearchValues).CA();
            if (!CharacterClassifier.IsDecimalDigit(self.Peek()[0])) return null;
            var next = await self.EnsureHasNonCharAsync(CharacterClassifier.DecimalDigitSearchValues).CA();
            if (!long.TryParse(self.Peek(next), out var ret)) return null;
            self.Advance(next);
            return ret;
        }
    }
}