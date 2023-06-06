using System;
using System.Collections.Generic;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.InterpreterState;

internal class PostscriptStack<T>: List<T>
{
    public void Push(T item) => Add(item);
    public T Peek() => this[^1];

    public T PeekAndPop()
    {
        var ret = Peek();
        Pop();
        return ret;
    }
    public void Pop()
    {
        RemoveAt(Count -1);
    }
}



internal class DictionaryStack : PostscriptStack<IPostscriptComposite>, IPostscriptComposite
{
    public void Push(PostscriptValue value) => Push(value.Get<IPostscriptComposite>());

    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result)
    {
        for (var i = Count-1; i >= 0; i--)
        {
            if (this[i].TryGet(indexOrKey, out result)) return true;
        }
        result = default;
        return false;
    }
}