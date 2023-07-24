using Melville.Postscript.Interpreter.Values;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.FileParsers;

namespace Melville.Pdf.LowLevel.Model.Objects2;

internal interface IIndirectValueSource: IPostscriptValueStrategy<string>
{
    ValueTask<PdfDirectValue> Lookup(MementoUnion memento);
    public bool TryGetObjectReference(out int objectNumber, out int generation, MementoUnion memento);

}

