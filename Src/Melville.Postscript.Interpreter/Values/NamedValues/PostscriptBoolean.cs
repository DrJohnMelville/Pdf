using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    /// <summary>
    /// This is a Value strategy for boolean values
    /// </summary>
    [StaticSingleton]
    public partial class PostscriptBoolean : 
        IPostscriptValueStrategy<string>, IPostscriptValueStrategy<bool>
    {
        string IPostscriptValueStrategy<string>.GetValue(in MementoUnion memento) =>
            Value(memento) ? "true" : "false";

        bool IPostscriptValueStrategy<bool>.GetValue(in MementoUnion memento) => Value(memento);

        private bool Value(in MementoUnion memento) => memento.Bools[0];
    }
}