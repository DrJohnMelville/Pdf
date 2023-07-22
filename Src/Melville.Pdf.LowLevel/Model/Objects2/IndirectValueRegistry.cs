using Melville.Postscript.Interpreter.Values;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.Objects2;

internal interface IIndirectValueSource: IPostscriptValueStrategy<string>
{
    ValueTask<PdfDirectValue> Lookup(MementoUnion memento);
    
}

