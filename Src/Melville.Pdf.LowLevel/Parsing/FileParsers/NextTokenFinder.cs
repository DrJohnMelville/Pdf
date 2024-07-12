using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class NextTokenFinder
{
    public static async ValueTask SkipToNextTokenAsync(IByteSource source)
    {
        do {} while (source.ShouldContinue(SkipToNextToken2(await source.ReadAsync().CA())));
    }

    private static (bool Success, SequencePosition Position) SkipToNextToken2(ReadResult source)
    {
        CheckForEndOfStream(source);
        var reader = new SequenceReader<byte>(source.Buffer);
        return (SkipToNextToken(ref reader), reader.Position);
    }

    private static void CheckForEndOfStream(ReadResult source)
    {
        if (source.IsCompleted && source.Buffer.IsEmpty)
            throw new PdfParseException("Read off end of stream.");
    }

    public static bool SkipToNextToken(this ref SequenceReader<byte> input)
    {
        if (!SkipToNextToken(ref input, out _)) return false;
        input.Rewind(1);
        return true;
    }

    public static bool SkipToNextToken(ref SequenceReader<byte> input, out byte firstByte)
    {
        while (true)
        {
            if (!input.TryRead(out firstByte)) return false;
            switch (CharClassifier.Classify(firstByte))
            {
                case CharacterClass.White:
                    break;
                case CharacterClass.Delimiter when firstByte == (byte) '%':
                    if (!input.TrySkipToEndOfLineMarker()) return false;
                    break;
                default:
                    return true;
            }
        }
    }
}