using System.Collections.Generic;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// Implements a Resource Library as defined in the Postscript Spec
/// </summary>
public readonly struct ResourceLibrary
{
    private readonly Dictionary<(PostscriptValue, PostscriptValue), PostscriptValue> items = new();

    /// <summary>
    /// Create a new ResourceLibrary
    /// </summary>
    public ResourceLibrary()
    {
    }

    /// <summary>
    /// Store a resource with a given category and key
    /// </summary>
    /// <param name="category">The category of the resource to be stored.</param>
    /// <param name="key">The key within that category for the resource to be stored.</param>
    /// <param name="value">The resource to store</param>
    public void Put(in PostscriptValue category, in PostscriptValue key, in PostscriptValue value) => 
        items[(category,key)] = value;

    /// <summary>
    /// Get a resource for a given category and key.
    /// </summary>
    /// <param name="category">The category of resource to get.</param>
    /// <param name="key">The key of the resource to get.</param>
    /// <returns>The resource with the given category and key.</returns>
    /// <exception cref="PostscriptNamedErrorException">If no resource exists for the given
    /// category and key.</exception>
    public PostscriptValue Get(in PostscriptValue category, in PostscriptValue key) =>
        items.TryGetValue((category, key), out var ret)
            ? ret
            : throw new PostscriptNamedErrorException("resource does not exist.", "resourceundefined");

    /// <summary>
    /// Remove a resource from the library
    /// </summary>
    /// <param name="category">Category of the item to remove.</param>
    /// <param name="key">Key of the item to remove</param>
    public void Undefine(in PostscriptValue category, in PostscriptValue key) => 
        items.Remove((category, key));

    /// <summary>
    /// Checks if a a resource exists for a given category/key combination.
    /// </summary>
    /// <param name="category">The category to check</param>
    /// <param name="key">The key to check</param>
    /// <returns>True if thee category and key map to a resource, false otherwise.</returns>
    public bool ContainsKey(PostscriptValue category, PostscriptValue key) => items.ContainsKey((category, key));
}