using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public ForAllCursor CreateForAllCursor();
}

/// <summary>
/// Helpers for IPostscriptComposite
/// </summary>
/// 
public static class PostscriptCompositeOperations 
{
    /// <summary>
    /// Wrap a postscriptcomposite as a postscript value
    /// </summary>
    /// <param name="dict">The composite to wrap</param>
    /// <returns></returns>
    public static PostscriptValue AsPostscriptValue(this IPostscriptComposite dict)=>
        new PostscriptValue(
            (IPostscriptValueStrategy<string>)dict,
            PostscriptBuiltInOperations.PushArgument, default);

    /// <summary>
    /// Try to get a a value  with a give key and cast to an expected type
    /// </summary>
    /// <typeparam name="T">The desired type of the output</typeparam>
    /// <param name="comp">The composite to get the value from</param>
    /// <param name="key">The key or index of the value</param>
    /// <param name="value">Out variable that receives the given ouput</param>
    /// <returns>True if the desired value exists and has the correct type, false otherwise</returns>
    public static bool TryGetAs<T>(
        this IPostscriptComposite comp, PostscriptValue key, [NotNullWhen(true)] out T? value)
    {
        value = default;
        return comp.TryGet(key, out var postscriptValue) &&
               postscriptValue.TryGet(out value);
    }
    /// <summary>
    /// Get a value of a given type from a compsite, and throw if this cannot be done.
    /// </summary>
    /// <typeparam name="T">The expected type  of the output</typeparam>
    /// <param name="comp">the composite to get the value from</param>
    /// <param name="key">The index or key to search for</param>
    /// <returns>The item corresponding to the key -- casted to the desired type</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public static T GetAs<T>(this IPostscriptComposite comp, PostscriptValue key) =>
        comp.TryGetAs(key, out T? value) ? value : 
            throw new KeyNotFoundException($"Cannot find key {key}");
}