using System.Collections.Generic;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This is a stack of dictionaries, which represents the defined functions
/// </summary>
public partial class DictionaryStack : 
    PostscriptStack<IPostscriptDictionary>
{
    /// <summary>
    /// Construct an empty DictionaryStack
    /// </summary>
    public DictionaryStack() : base(3,"dict")
    {
    }

    /// <summary>
    /// Add a IPostscriptComposite
    /// </summary>
    /// <param name="value"></param>
    public void Push(PostscriptValue value) => Push(value.Get<IPostscriptDictionary>());

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

    /// <summary>
    /// Overwrite the topmost instance of key with the given value.  If the key
    /// does not exist in the dictionary stack, add to the topmost dictionary.
    /// </summary>
    /// <param name="key">The key to store under.</param>
    /// <param name="value">rhe value to store</param>
    public void Store(scoped in PostscriptValue key, scoped in PostscriptValue value)
    {
        for (int i = Count - 1; i >= 0; i--)
        {
            var dict = this[i];
            if (TryReplaceValue(key, value, dict)) return;
        }
        Peek().Put(key, value);
    }

    private static bool TryReplaceValue(
        scoped in PostscriptValue key, scoped in PostscriptValue value, 
        IPostscriptComposite dict)
    {
        if (dict.TryGet(key, out _))
        {
            dict.Put(key, value);
            return true;
        }

        return false;
    }

    internal void PostscriptWhere(OperandStack stack)
    {
        // Find the dictionary that contains a given key
        var key = stack.Pop();
        for (int i = Count - 1; i >= 0; i--)
        {
            var dict = this[i];
            if (dict.TryGet(key, out _))
            {
                stack.Push(dict.AsPostscriptValue());
                stack.Push(true);
                return;
            }
        }
        stack.Push(false);
    }

    internal PostscriptValue CurrentDictAsValue => Peek().AsPostscriptValue();
   
    internal PostscriptValue WriteStackTo(PostscriptArray array)
    {
        var targetSpan = array.AsSpan();
        var sourceSpan = this.CollectionAsSpan();
        for (int i = 0; i < sourceSpan.Length; i++)
        {
            targetSpan[i] = sourceSpan[i].AsPostscriptValue();
        }

        return
            array.InitialSubArray(sourceSpan.Length, PostscriptBuiltInOperations.PushArgument);
    }

    internal void ResetToBottom3()
    {
        while (Count > 3) Pop();
    }

    /// <summary>
    /// Dumps the dictionary stack for placement in the $error dictionary
    /// </summary>
    /// <returns></returns>
    public PostscriptValue[] DumpTrace()
    {
        var ret = new PostscriptValue[Count];
        for (int i = 0; i < Count; i++)
        {
            ret[i] = new PostscriptValue(this[i], PostscriptBuiltInOperations.PushArgument, default);
        }
        return ret;
    }
}   