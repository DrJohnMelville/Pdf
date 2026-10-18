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
        using var _ = Force.Using(source.SubsetIAsyncReader(xrefPosition), out var reader);
        using var buffer = new ParsingBuffer(reader);
        if (!await TrySkipToken(buffer, xrefTag)) return null;
        await buffer.SkipOverAsync(CharacterClassifier.WhitespaceSearchValues);
        await new XrefTableParser(buffer, source.NewIndirectResolver).Parse();
        if (!await TrySkipToken(buffer, trailerTag))
            throw new PdfParseException("Trailer does not follow xref");

        var trailerIndirect = (await new RootObjectParser(
            new ParsingReader(source, buffer, null!)
            ).ParseAsync().CA());
        if (!trailerIndirect.TryGetEmbeddedDirectValue(out PdfDictionary? trailer))
            throw new PdfParseException("Trailer dictionary is invalid");
        return trailer;
    }
    private async ValueTask<bool> TrySkipToken(ParsingBuffer buffer, byte[] token)
    {
        await buffer.EnsureBytesAsync(token.Length);
        if (!buffer.Peek().StartsWith(token)) return false;
        buffer.Advance(token.Length);
        return true;
    }
    private async Task<PdfDictionary?> ReadSingleRefTrailerBlockAsyncOld ()
    {
        using var context = source.SubsetReader(xrefPosition);
        if (!await TokenChecker.CheckTokenAsync(context.ObsoleteReader, xrefTag).CA()) return null;
        await NextTokenFinder.SkipToNextTokenAsync(context.ObsoleteReader).CA();
        await new CrossReferenceTableParser(context.ObsoleteReader, context.Owner.NewIndirectResolver).ParseAsync().CA();
        await NextTokenFinder.SkipToNextTokenAsync(context.ObsoleteReader).CA();
        if (!await TokenChecker.CheckTokenAsync(context.ObsoleteReader, trailerTag).CA())
            throw new PdfParseException("Trailer does not follow xref");
        var trailerIndirect = (await new RootObjectParser(context).ParseAsync().CA());
        if (!trailerIndirect.TryGetEmbeddedDirectValue(out PdfDictionary? trailer))
            throw new PdfParseException("Trailer dictionary is invalid");
        return trailer;
    }

    private static readonly byte[] xrefTag = {120, 114, 101, 102}; // xref;
    private static readonly byte[] trailerTag = {(byte)'t',114, 97, 105, 108, 101, 114}; // trailer
}