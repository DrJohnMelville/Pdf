using System.Collections.Generic;

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
    /// <param name="source">Value to copy from</param>
    /// <returns>The value to be pushed on the stack as the result of the copy operation</returns>
    PostscriptValue CopyFrom(PostscriptValue source);
}

/// <summary>
/// This interface describes Postscript Arrays arrays and strings are both arrays
/// </summary>
public interface IPostscriptArray : IPostscriptComposite
{
    /// <summary>
    /// /// Get a subsequence of the given object.
    /// </summary>
    /// <param name="beginningPosition">index of the first position</param>
    /// <param name="length"></param>
    /// <returns></returns>
    public IPostscriptValueStrategy<string> IntervalFrom(int beginningPosition, int length);

    /// <summary>
    /// Overwrite 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="values"></param>
    public void InsertAt(int index, IPostscriptArray values);
}

/// <summary>
/// Additional operations on IPostscriptComposite
/// </summary>
public static class PostscriptCompositeImpl
{
    /// <summary>
    /// Get an item from the composite or throw if it does not exist.
    /// </summary>
    /// <param name="composite">The composite to search for</param>
    /// <param name="indexOrKey">The index or key of the item.</param>
    /// <returns>The item at the given index or key.</returns>
    /// <exception cref="KeyNotFoundException">If the given index or key does not exist.</exception>
    public static PostscriptValue Get(
        this IPostscriptComposite composite, in PostscriptValue indexOrKey) =>
        composite.TryGet(indexOrKey, out var result)
            ? result
            : throw new KeyNotFoundException($"Cannot find key {indexOrKey}.");
}