using System.Threading.Tasks;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects;

internal readonly partial struct DeferredPdfHolder
{
    [FromConstructor] private readonly IIndirectObjectSource strategy;
    [FromConstructor] private readonly MementoUnion memento;

    public ValueTask<PdfDirectObject> GetAsync() => strategy.LookupAsync(memento);
}
