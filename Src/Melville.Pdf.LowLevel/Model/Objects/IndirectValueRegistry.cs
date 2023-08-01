using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal interface IIndirectValueSource: IPostscriptValueStrategy<string>, IPostscriptValueStrategy<DeferredPdfHolder>
{
    ValueTask<PdfDirectValue> LookupAsync(MementoUnion memento);
    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento);

    DeferredPdfHolder IPostscriptValueStrategy<DeferredPdfHolder>.GetValue(
        in MementoUnion memento) => new DeferredPdfHolder(this, memento);

}

internal readonly partial struct DeferredPdfHolder
{
    [FromConstructor] private readonly IIndirectValueSource strategy;
    [FromConstructor] private readonly MementoUnion memento;

    public ValueTask<PdfDirectValue> GetAsync() => strategy.LookupAsync(memento);
}
