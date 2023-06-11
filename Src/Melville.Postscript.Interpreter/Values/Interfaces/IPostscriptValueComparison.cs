using System;

namespace Melville.Postscript.Interpreter.Values
{
    internal interface IPostscriptValueComparison
    {
        public bool Equals(in Int128 memento, object otherStrategy, in Int128 otherMemento);
    }
}