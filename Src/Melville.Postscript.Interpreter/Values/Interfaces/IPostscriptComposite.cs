using System;
using System.Diagnostics;
using Melville.INPC;
using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// This interface implements the common elements of dictionaries and arrays
/// </summary>
public interface IPostscriptComposite
{
    /// <summary>
    /// Attempt to retrieve a value from a composite.
    /// </summary>
    /// <param name="indexOrKey">The index or key of the desired item</param>
    /// <param name="result">Out variable receiving the result of the operation</param>
    /// <returns>True if the item is found, false otherwise.</returns>
    bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result);

    /// <summary>
    /// Add an item to the Composite.
    /// </summary>
    /// <param name="indexOrKey">The index or key to add under.</param>
    /// <param name="value">The value to add to the dictionary.</param>
    void Put(in PostscriptValue indexOrKey, in PostscriptValue value);

    /// <summary>
    /// Number of items in the composite
    /// </summary>
    int Length { get; }

    /// <summary>
    /// Executes the postscript copy operation (which is odd) and returns the value to
    /// be pushed back on the stack
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target">Value to copy from</param>
    /// <returns>The value to be pushed on the stack as the result of the copy operation</returns>
    PostscriptValue CopyFrom(PostscriptValue source, PostscriptValue target);

    /// <summary>
    /// Creates a forall cursor for this composite.
    /// </summary>
    public ForAllCursor CreateForAllCursor();
}

public readonly partial struct ForAllCursor
{
    /// <summary>
    /// A Memory containing the items to be iterated over.
    /// </summary>
    [FromConstructor] private readonly Memory<PostscriptValue> items;
    /// <summary>
    /// Indicates the number of items pushed per iteration.
    /// </summary>
    [FromConstructor] public int ItemsPer { get; }

    partial void OnConstructed()
    {
        Debug.Assert(items.Length % ItemsPer == 0);
    }

    /// <summary>
    /// Try to do another iteration of the ForAll operation.
    /// </summary>
    /// <param name="engine">Postscript engine we are running on</param>
    /// <param name="index">A scratch value that starts at 0 and the object uses
    /// as iteration state.</param>
    /// <returns></returns>
    public bool TryGetItr(in Span<PostscriptValue> target, ref int index)
    {
        if (index >= items.Length) return false;
        items.Span.Slice(index, ItemsPer).CopyTo(target);
        index += ItemsPer;
        return true;
    }
}