using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers.IndirectValues;

internal partial class UnenclosedDeferredPdfStrategy : IIndirectObjectSource
{
    [FromConstructor] private readonly ParsingFileOwner owner;

    public string GetValue(in MementoUnion memento) => 
        $"Raw Offset Reference @{memento.UInt64s[1]}";

    public async ValueTask<PdfDirectObject> LookupAsync(MementoUnion memento)
    {
        var objectNumber = memento.Int32s[0];
        var generation = memento.Int32s[1];
        var offset = memento.Int64s[1];
        using var reader = owner.RentReader(offset, objectNumber, generation);
        var result = await new RootObjectParser(reader).ParseTopLevelObjectAsync().CA();
        return result;
    }

    public bool TryGetObjectReference(
        out int objectNumber, out int generation, MementoUnion memento)
    {
        objectNumber = memento.Int32s[0];
        generation = memento.Int32s[1];
        return true;
    }

    public PdfIndirectObject Create(long offset, int number, int generation) => 
        new(this, MementoUnion.CreateFrom(number, generation, offset));
}