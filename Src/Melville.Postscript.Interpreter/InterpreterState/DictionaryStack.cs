using System.Collections.Generic;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is a stack of dictionaries, which represents the defined functions
/// </summary>
public partial class DictionaryStack : 
    PostscriptStack<IPostscriptComposite>
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

    /// <summary>
    /// Try to get an item from the composite dictionary
    /// </summary>
    /// <param name="indexOrKey">Key of the item to search for</param>
    /// <param name="result">Receives the value of the searched key</param>
    /// <returns>True if a value is found, false otherwise</returns>
    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result)
    {
        for (var i = Count - 1; i >= 0; i--)
        {
            if (this[i].TryGet(indexOrKey, out result)) return true;
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Get a value from the stack of dictionaries, searching top dictionary first
    /// </summary>
    /// <param name="key">The key to search for</param>
    /// <returns>The value</returns>
    /// <exception cref="KeyNotFoundException">If the given key is nout found.</exception>
    public PostscriptValue Get(in PostscriptValue key)=>
        TryGet(key, out var result)
            ? result
            : throw new KeyNotFoundException($"Cannot find key {key}.");


    /// <summary>
    /// Add an item to the top dictionary in the stack
    /// </summary>
    /// <param name="indexOrKey">key to store the value under</param>
    /// <param name="value">The value to store</param>
    public void Put(in PostscriptValue indexOrKey, in PostscriptValue value) =>
        Peek().Put(indexOrKey, value);
}