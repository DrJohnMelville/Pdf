using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class PdfDictionaryAndStreamParser : IPdfObjectParser
{
    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        var dictionary = await PdfParserParts.Dictionary.ParseDictionaryItemsAsync(source).CA();
        bool isStream;
        do {}while (source.Reader.ShouldContinue(CheckForStreamTrailer(await source.Reader.ReadAsync().CA(), out isStream)));
        return CreateFinalObject(source, dictionary, isStream);
    }

    private static PdfObject CreateFinalObject(
        IParsingReader source, Memory<KeyValuePair<PdfName, PdfObject>> dictionary, bool isStream) =>
        isStream ? 
            ConstrutStream(source, dictionary) : 
            new PdfDictionary(dictionary);

    private static PdfStream ConstrutStream(
        IParsingReader source, Memory<KeyValuePair<PdfName, PdfObject>> dictionary) =>
        new(new InlineStreamSource(source.Reader.GlobalPosition, source.Owner, source.ObjectCryptContext()),
            dictionary);

    private static byte[] streamSuffix = {115, 116, 114, 101, 97, 109}; // stream
    private (bool success, SequencePosition pos) CheckForStreamTrailer(ReadResult source, out bool isStream)
    {
        isStream = false;
        var reader = new SequenceReader<Byte>(source.Buffer);
        if (!FindStreamSuffix(ref reader)) return (source.IsCompleted, reader.Position);
        if (!reader.TryCheckToken(streamSuffix, source.IsCompleted, out isStream)) return (source.IsCompleted, reader.Position);
        if (!isStream) return (true, source.Buffer.Start);
        if (!SkipOverStreamSuffix(ref reader)) return (false, reader.Position);
        isStream = true;
        return (true, reader.Position);
    }

    private static bool FindStreamSuffix(ref SequenceReader<byte> reader) => reader.SkipToNextToken();
    
    private const byte carriageReturn = 0x0D;
    private const byte lineFeed = 0x0A;
    private static readonly byte[] endOfLine = { carriageReturn, lineFeed };

    //The PDF Spec section 7.8.3.1 explicitly states that the Stream keyword wil be terminated by either a
    // CR/LF sequence or a LF alone, because otherwise we would not know if the LF after the CR was a line terminator
    // or the first character of the stream.  In fact, though PDFs that use CR alone exist and are parsed successfully by
    // PDF reader.  Thus if we find a CR we will look for a following LF, but if the next character is not a LF we
    // presume it is the first character of the stream.
    private static bool SkipOverStreamSuffix(ref SequenceReader<byte> reader)
    {
        if (!GetNextDelimiter(ref reader, out var delim)) return false;
        if (delim is lineFeed) return true;
        return TrySkipOptionalLineFeed(ref reader);
    }

    private static bool GetNextDelimiter(ref SequenceReader<byte> reader, out byte value)
    {
        value = 0;
        return reader.TryReadToAny(out ReadOnlySequence<byte> _, endOfLine, false) &&
               reader.TryRead(out value);
    }

    private static bool TrySkipOptionalLineFeed(ref SequenceReader<byte> reader)
    {
        if (!reader.TryPeek(0, out var item)) return false;
        if (item == lineFeed)
        {
            reader.Advance(1);
        }
        return true;
    }
}