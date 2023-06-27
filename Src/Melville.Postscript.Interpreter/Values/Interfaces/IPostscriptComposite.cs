using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Execution;

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
    internal ForAllCursor CreateForAllCursor();
}

internal static class PostscriptCompositeOperations 
{
    public static PostscriptValue AsPostscriptValue(this IPostscriptComposite dict)=>
        new PostscriptValue(
            (IPostscriptValueStrategy<string>)dict,
            PostscriptBuiltInOperations.PushArgument, 0);
}