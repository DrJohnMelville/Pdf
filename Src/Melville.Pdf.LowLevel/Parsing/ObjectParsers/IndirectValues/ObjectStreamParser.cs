using Melville.Parsing.IStreamAdaptors;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using Melville.StreamInterfaces.Streams;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

internal readonly struct  ObjectStreamParsingMethodObject(
    int count, int initialOffset, LimitedReader limiter, ParsingBuffer buffer, int[] numbers,
    ParsingFileOwner owner, Action<int, PdfDirectObject> reportObj)
{
    public async ValueTask InnerParseAsync()
    {
        // Read the object stream header
        await ReadHeader().CA();

        var rootObjectParser = CreateRootParser();

        for (int i = 0; i < count; i++)
        {
            PdfIndirectObject value = await ReadValue(rootObjectParser, i).CA();
            if (!value.TryGetEmbeddedDirectValue(out var directValue))
                throw new PdfParseException("Cannot have a bare reference in an object stream");
            reportObj(ObjectNumber(i), directValue);
        }
    }

    private async Task<PdfIndirectObject> ReadValue(RootObjectParser rootObjectParser, int i)
    {
        limiter.LengthLimit = initialOffset + numbers[LengthIndex(i + 1)];
        await buffer.AdvanceToRelativePositionAsync(initialOffset + numbers[LengthIndex(i)]);
        var value = await rootObjectParser.ParseAsync().CA();
        return value;
    }

    private int ObjectNumber(int i) => numbers[2 * i];

    private RootObjectParser CreateRootParser() =>
        new RootObjectParser(new ParsingReader(owner, buffer, EmptyCodeSource.Instance));

    private async Task ReadHeader()
    {
        limiter.LengthLimit = initialOffset;
        await buffer.EnsureBytesAsync(initialOffset).CA(); ;
        ReadNumbers(buffer.Peek(), count, numbers);
        numbers[LengthIndex(count)] = int.MaxValue - initialOffset;
    }

    private int LengthIndex(int i) => 1 + (2 * i);

    void ReadNumbers(Span<byte> peek, int count, int[] numbers)
    {
        var reader = new SpanParsingBuffer(peek);
        int i = 0;
        while (i < count * 2)
        {
            if (!reader.TryGetPostscriptToken(out var token))
                throw new PdfParseException("Insufficient numbers in an object stream");
            if (token.TryGet<int>(out var number))
                numbers[i++] = number;

        }
    }
}

internal class ObjectStreamParser(
    ParsingFileOwner owner, int sourceObjectNumber, int desiredObjectNumber, 
    PdfStream sourceStream)
{
    private readonly ObjectStreamCostume source = new(sourceStream);
    private PdfDirectObject result = default;

    private void ReportItem(int objectNum, PdfDirectObject value)
    {
        owner.IndirectResolver.TryRegisterFromObjectStream(
                    sourceObjectNumber, objectNum, value);
        if (objectNum == desiredObjectNumber)
            result = value;

    }

    public async ValueTask<PdfDirectObject> ParseAsync()
    {
        var count = await source.CountAsync().CA();
        var initialOffset = await source.FirstValueOffsetAsync().CA();
        using var stream = await source.StreamAsync().CA();
        var limiter = new LimitedReader(stream.AsStreamReader());
        using var buffer = new ParsingBuffer(limiter);
        using var numbers = new PooledBuffer<int>((count * 2)+2);

        await new ObjectStreamParsingMethodObject(count, initialOffset, limiter,
            buffer, numbers, owner, ReportItem).InnerParseAsync().CA();

        return result;
    }

}
