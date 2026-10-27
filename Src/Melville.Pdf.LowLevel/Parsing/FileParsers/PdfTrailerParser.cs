using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal record SingleLinkedList<T>(T Value, SingleLinkedList<T>? Next) where T : IEquatable<T>;
internal static class SingleLinkedListImpl
{
    public static bool Contains<T>(this SingleLinkedList<T>? items, T key) where T:IEquatable<T>
    {
        if (items is null) return false;
        return items.Value.Equals(key) ||
                items.Next.Contains(key);
    }
}

internal readonly struct PdfTrailerParser(
    ParsingFileOwner source, long xrefPosition, SingleLinkedList<long>? priorPositions = null)
{
    public Task<PdfDictionary> ParseXrefAndTrailerAsync() => 
        XrefAndTrailerAsync();

    private async Task<PdfDictionary> XrefAndTrailerAsync()
    {

        if (xrefPosition >= source.StreamLength)
            throw new PdfParseException("Xref location is invalid");
        var trailerDictionary = await ReadSingleRefTrailerBlockAsync().CA();

        if (trailerDictionary != null)
        {
            await source.InitializeDecryptionAsync(trailerDictionary).CA();
        }
        else
        {
            trailerDictionary = await CrossReferenceStreamParser.ReadAsync(source, xrefPosition).CA();
        }

        if ((await trailerDictionary.GetOrDefaultAsync(KnownNames.Prev, -1L).CA()) is var offset && 
            offset >= 0 && !priorPositions.Contains(offset))
        {
            await new PdfTrailerParser(
                source, offset, new SingleLinkedList<long>(xrefPosition, priorPositions))
                .ParseXrefAndTrailerAsync().CA();
        }
        return  trailerDictionary;
    }

    private async Task<PdfDictionary?> ReadSingleRefTrailerBlockAsync()
    {
        using var parsingReader = source.SubsetReader(xrefPosition);

        using var buffer = parsingReader.Buffer;

        //if xref token then we need to read an xref table, otherwise it is an xref stream
        if (!await TrySkipTokenAsync(buffer, xrefTag)) return null;
        await buffer.SkipOverAsync(CharacterClassifier.WhitespaceSearchValues);
        
        await new XrefTableParser(buffer, source.IndirectResolver).ParseAsync();
        
        if (!await TrySkipTokenAsync(buffer, trailerTag))
            throw new PdfParseException("Trailer does not follow xref");

        var trailerIndirect = (await new RootObjectParser(parsingReader).ParseAsync().CA());
        if (!trailerIndirect.TryGetEmbeddedDirectValue(out PdfDictionary? trailer))
            throw new PdfParseException("Trailer dictionary is invalid");
        
        return trailer;
    }
    private async ValueTask<bool> TrySkipTokenAsync(ParsingBuffer buffer, byte[] token)
    {
        await buffer.EnsureBytesAsync(token.Length);
        if (!buffer.Peek().StartsWith(token)) return false;
        buffer.Advance(token.Length);
        return true;
    }

    private static readonly byte[] xrefTag = {120, 114, 101, 102}; // xref;
    private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
}