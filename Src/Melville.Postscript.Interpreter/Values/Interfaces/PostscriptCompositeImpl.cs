using System.Collections.Generic;

namespace Melville.Postscript.Interpreter.Values;

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