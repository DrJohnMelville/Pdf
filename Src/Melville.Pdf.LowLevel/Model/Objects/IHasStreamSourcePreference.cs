using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Model.Objects
{
    internal interface IHasStreamSourcePreference
    {
        bool ComesFromStream(int objectStreamNumber, MementoUnion memento);
    }
}