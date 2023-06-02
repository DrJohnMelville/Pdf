using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    [StaticSingleton]
    internal partial class PostscriptBoolean : 
        IPostscriptValueStrategy<string>, IPostscriptValueStrategy<bool>
    {
        string IPostscriptValueStrategy<string>.GetValue(in Int128 memento) =>
            Value(memento) ? "true" : "false";

        bool IPostscriptValueStrategy<bool>.GetValue(in Int128 memento) => Value(memento);

        private bool Value(in Int128 memento) => memento != 0;
    }
}