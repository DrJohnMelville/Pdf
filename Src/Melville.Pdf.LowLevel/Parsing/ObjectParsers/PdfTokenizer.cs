using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;


public readonly partial struct PdfTokenizer
{
    [FromConstructor] private readonly IByteSource source;

    public async ValueTask<PdfDirectValue> NextTokenAsync()
    {
        while (true)
        {
            var readResult = await source.ReadAsync().CA();
            if (TryParseToken(readResult, out var result)) return result;
            source.MarkSequenceAsExamined();
        }
    }

    private bool TryParseToken(ReadResult readResult, out PdfDirectValue result)
    {
        var reader = new SequenceReader<byte>(FinalSequence(readResult));
        if (!ParseToken(ref reader, out result)) return false;
        source.AdvanceTo(readResult.Buffer.GetPosition(reader.Consumed));
        return true;
    }

    private static ReadOnlySequence<byte> FinalSequence(ReadResult readResult) =>
        readResult.IsCompleted ? readResult.Buffer.AppendCR() : readResult.Buffer;

    private bool ParseToken(ref SequenceReader<byte> reader, out PdfDirectValue result)
    {
        if (!reader.TryPeekNextNonComment(out byte value))
            return (default(PdfDirectValue)).AsFalseValue(out result);
        reader.Advance(1);
        return (char)value switch
        {
            '(' => ParseString<SyntaxStringDecoder, int>(ref reader, out result),
            '<' => HandleOpenWakka(ref reader, out result),
            '>' => HandleCloseWakka(ref reader, out result),
            '/' => PdfNameTokenizer.Parse(ref reader, out result),
            '[' => ((PdfDirectValue)PdfParsingCommand.PushMark).AsTrueValue(out result),
            ']' => ((PdfDirectValue)PdfParsingCommand.CreateArray).AsTrueValue(out result),
            _ => TryParseUnprefixedItem(ref reader, out result)
        };
    }

    private bool HandleOpenWakka(ref SequenceReader<byte> reader, out PdfDirectValue result)
    {
        if (!reader.TryPeek(out var character)) 
            return default(PdfDirectValue).AsFalseValue(out result);
        if (character != (byte)'<')
            return ParseString<HexStringDecoder, byte>(ref reader, out result);

        reader.Advance(1);
        return ((PdfDirectValue)PdfParsingCommand.PushMark).AsTrueValue(out result);
    }
    private bool HandleCloseWakka(ref SequenceReader<byte> reader, out PdfDirectValue result)
    {
        if (!reader.TryRead(out var secondChar))
            return default(PdfDirectValue).AsFalseValue(out result);
        if (secondChar != (byte)'>')
            throw new PdfParseException("Unexpected Character following >: " + (char)secondChar);

        return ((PdfDirectValue)PdfParsingCommand.CreateDictionary).AsTrueValue(out result);

    }

    private bool ParseString<T, TState>(ref SequenceReader<byte> reader, out PdfDirectValue result)
       where T: IStringDecoder<TState>, new() where TState: new()
    {
        return new StringTokenizer<T, TState>().Parse(ref reader, out PostscriptValue psStr)
            ? ToDirectValue(psStr).AsTrueValue(out result)
            : default(PdfDirectValue).AsFalseValue(out result);
    }

    private bool TryParseUnprefixedItem(ref SequenceReader<byte> reader, out PdfDirectValue result)
    {
        reader.Rewind(1);
        if (!reader.TryReadToAny(out ReadOnlySpan<byte> value,
                CharacterClassifier.DelimiterChars(), false))
            return default(PdfDirectValue).AsFalseValue(out result);
        result = RecognizeItem(value);
        return true;
    }

    private PdfDirectValue RecognizeItem(ReadOnlySpan<byte> value) => value switch
    {
        _ when NumberTokenizer.TryDetectNumber(value, out var psNum) => ToDirectValue(psNum),
        _ when "true"u8.SequenceEqual(value) => true,
        _ when "false"u8.SequenceEqual(value) => false,
        _ when "null"u8.SequenceEqual(value) => PdfDirectValue.CreateNull(),
        _ when "obj"u8.SequenceEqual(value) => PdfParsingCommand.ObjOperator,
        _ when "stream"u8.SequenceEqual(value) => PdfParsingCommand.StreamOperator,
        _ when "endstream"u8.SequenceEqual(value) => PdfParsingCommand.EndStreamOperator,
        _ when "endobj"u8.SequenceEqual(value) => PdfParsingCommand.EndObject,
        _ when "R"u8.SequenceEqual(value) => PdfParsingCommand.CreateReference,
        _ => throw new PdfParseException($"Unrecognized Token: {value.ExtendedAsciiString()}")
    };

    private static PdfDirectValue ToDirectValue(PostscriptValue psNum) => new(psNum.ValueStrategy, psNum.Memento);
}