using System.Threading.Tasks;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal interface IIndirectObjectSource: IPostscriptValueStrategy<string>, IPostscriptValueStrategy<DeferredPdfHolder>
{
    ValueTask<PdfDirectObject> LookupAsync(MementoUnion memento);
    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento);

    DeferredPdfHolder IPostscriptValueStrategy<DeferredPdfHolder>.GetValue(
        in MementoUnion memento) => new DeferredPdfHolder(this, memento);

}