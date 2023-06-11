using System.Collections.Generic;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is a stack of dictionaries, which represents the defined functions
/// </summary>
public partial class DictionaryStack : 
    PostscriptStack<IPostscriptComposite>, IPostscriptComposite
{
    /// <summary>
    /// Construct an empty DictionaryStack
    /// </summary>
    public DictionaryStack() : base(3)
    {
    }

    /// <summary>
    /// Add a IPostscriptComposite
    /// </summary>
    /// <param name="value"></param>
    public void Push(PostscriptValue value) => Push(value.Get<IPostscriptComposite>());

    /// <inheritdoc />
    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result)
    {
        for (var i = Count - 1; i >= 0; i--)
        {
            if (this[i].TryGet(indexOrKey, out result)) return true;
        }
        result = default;
        return false;
    }

    /// <inheritdoc />
    public void Add(in PostscriptValue indexOrKey, in PostscriptValue value) =>
        Peek().Add(indexOrKey, value);
}