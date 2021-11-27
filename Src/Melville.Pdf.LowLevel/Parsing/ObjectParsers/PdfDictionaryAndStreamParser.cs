using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class PdfDictionaryAndStreamParser : IPdfObjectParser
{
    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        var dictionary = await PdfParserParts.Dictionary.ParseDictionaryItemsAsync(source);
        bool isStream;
        do {}while (source.Reader.Source.ShouldContinue(CheckForStreamTrailer(await source.Reader.Source.ReadAsync(), out isStream)));
        return CreateFinalObject(source, dictionary, isStream);
    }

    private static PdfObject CreateFinalObject(
        IParsingReader source, Dictionary<PdfName, PdfObject> dictionary, bool isStream) =>
        isStream ? 
            ConstrutStream(source, dictionary) : 
            new PdfDictionary(dictionary);

    private static PdfStream ConstrutStream(
        IParsingReader source, Dictionary<PdfName, PdfObject> dictionary) =>
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

    private static bool FindStreamSuffix(ref SequenceReader<byte> reader) => 
        NextTokenFinder.SkipToNextToken(ref reader);

    private static bool SkipOverStreamSuffix(ref SequenceReader<byte> reader) => 
        reader.TryAdvanceTo((byte)'\n');
}