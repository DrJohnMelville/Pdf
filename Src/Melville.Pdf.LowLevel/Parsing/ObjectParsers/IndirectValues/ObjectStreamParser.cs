using Melville.Parsing.IStreamAdaptors;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Adapters;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;


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
        using var _ = Force.Using(await source.ReaderAsync().CA(), out var stream);
        var limiter = new LimitedReader(stream);
        using var buffer = new ParsingBuffer(limiter);
        var parsingReader = new ParsingReader(owner, buffer);
        using var numbers = new PooledBuffer<int>((count * 2)+2);

        await new ObjectStreamParsingMethodObject(count, initialOffset, limiter,
            numbers, parsingReader, this).InnerParseAsync().CA();

        return result;
    }

    private readonly struct ObjectStreamParsingMethodObject(
        int count, int initialOffset, LimitedReader limiter, int[] numbers,
        ParsingReader pr, ObjectStreamParser outer)
    {
        public async ValueTask InnerParseAsync()
        {
            // Read the object stream header
            await ReadHeaderAsync().CA();

            var rootObjectParser = CreateRootParser();

            for (int i = 0; i < count; i++)
            {
                PdfIndirectObject value = await ReadValue(rootObjectParser, i).CA();
                if (!value.TryGetEmbeddedDirectValue(out var directValue))
                    throw new PdfParseException("Cannot have a bare reference in an object stream");
                outer.ReportItem(ObjectNumber(i), directValue);
            }
        }

        private async Task<PdfIndirectObject> ReadValue(RootObjectParser rootObjectParser, int i)
        {
            limiter.LengthLimit = initialOffset + numbers[LengthIndex(i + 1)];
            await pr.Buffer.AdvanceToRelativePositionAsync(initialOffset + numbers[LengthIndex(i)]);
            var value = await rootObjectParser.ParseAsync().CA();
            return value;
        }

        private int ObjectNumber(int i) => numbers[2 * i];

        private RootObjectParser CreateRootParser() =>
            new RootObjectParser(pr);

        private async Task ReadHeaderAsync()
        {
            limiter.LengthLimit = initialOffset;
            await pr.Buffer.EnsureBytesAsync(initialOffset).CA(); ;
            ReadNumbers(pr.Buffer.Peek(), count, numbers);
            numbers[LengthIndex(count)] = int.MaxValue - initialOffset;
        }

        private int LengthIndex(int i) => 1 + (2 * i);

        void ReadNumbers(ReadOnlySpan<byte> peek, int count, int[] numbers)
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


}
