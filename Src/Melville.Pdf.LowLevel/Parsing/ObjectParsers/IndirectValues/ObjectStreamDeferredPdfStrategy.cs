using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

internal partial class ObjectStreamDeferredPdfStrategy : IIndirectValueSource
{
    [FromConstructor] private readonly ParsingFileOwner owner;

    public string GetValue(in MementoUnion memento) =>
        $"Load from Object Stream # {memento.Int32s[0]} as position {memento.Int32s[1]}";

    public async ValueTask<PdfDirectValue> LookupAsync(MementoUnion memento)
    {
        var source = (await owner.NewIndirectResolver
            .LookupAsync(memento.Int32s[0], 0).CA()).Get<PdfValueStream>();
        int desiredOrdinal = (int)memento.Int64s[1];

        return await ReadObjectStream(source, desiredOrdinal).CA();
    }

    private async Task<PdfDirectValue> ReadObjectStream(
        PdfValueStream source, int desiredObjectNumber)
    {
        var ret = await TryReadExtendsStream(source, desiredObjectNumber).CA();

        var parser = await ObjectStreamParser.CreateAsync(owner, source, desiredObjectNumber).CA();

        return await parser.ParseAsync(ret).CA();
    }

    private async Task<PdfDirectValue> TryReadExtendsStream(
        PdfValueStream source, int desiredObjectNumber) =>
        source.TryGetValue(KnownNames.ExtendsTName, out var task) &&
        (await task).TryGet(out PdfValueStream? innerStream)
            ? await ReadObjectStream(innerStream, desiredObjectNumber).CA()
            : default;

    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento)
    {
        objectNumber = generation = -1;
        return false;
    }

    public PdfIndirectValue Create(int streamNum, int streamPosition, int number) => 
        new(this, MementoUnion.CreateFrom(streamNum, streamPosition, number));
}