using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Tokenizers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;


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
        return (char)value switch
        {
            _ => TryParseUnprefixedItem(ref reader, out result)
        };

    }

    private bool TryParseUnprefixedItem(ref SequenceReader<byte> reader, out PdfDirectValue result)
    {
        if (!reader.TryReadToAny(out ReadOnlySpan<byte> value,
                CharacterClassifier.DelimiterChars(), false))
            return default(PdfDirectValue).AsFalseValue(out result);
        result = RecognizeItem(value);
        return true;
    }

    private PdfDirectValue RecognizeItem(ReadOnlySpan<byte> value) => value switch
    {
        _ when NumberTokenizer.TryDetectNumber(value, out var psNum) => 
            new PdfDirectValue(psNum.ValueStrategy, psNum.Memento),
        _ when "true"u8.SequenceEqual(value) => true,
        _ when "false"u8.SequenceEqual(value) => false,
        _ => throw new PdfParseException($"Unrecognized Token: {value.ExtendedAsciiString()}")
    };

}