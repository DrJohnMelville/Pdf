using System;

namespace Melville.Postscript.Interpreter.Values
{
    internal interface IPostscriptValueComparison
    {
        public bool Equals(in MementoUnion memento, object otherStrategy, in MementoUnion otherMemento);
    }
}