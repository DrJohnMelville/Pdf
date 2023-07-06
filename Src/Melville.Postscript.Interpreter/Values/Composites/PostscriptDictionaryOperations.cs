using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values.Composites;

/// <summary>
/// Helpers for PdfDictionaries
/// </summary>
public static class PostscriptDictionaryOperations
{
    /// <summary>
    /// Add a value to a composite and return it.
    /// </summary>
    /// <typeparam name="T">A IComposite implementing type to add the value to</typeparam>
    /// <param name="dict">The dictionary to modify</param>
    /// <param name="key">The key to associate the item with.</param>
    /// <param name="value">The item to insert</param>
    /// <returns>The composite passed in the first argument</returns>
    public static T With<T>(this T dict, in PostscriptValue key, in PostscriptValue value) 
        where T:IPostscriptComposite
    {
        dict.Put(key, value);
        return dict;
    }

    /// <summary>
    /// Add a value to a composite and return it.
    /// </summary>
    /// <typeparam name="T">A IComposite implementing type to add the value to</typeparam>
    /// <param name="dict">The dictionary to modify</param>
    /// <param name="key">The key to associate the item with.</param>
    /// <param name="value">The item to insert</param>
    /// <returns>The composite passed in the first argument</returns>
    public static T With<T>(this T dict, in PostscriptValue key, IExternalFunction value) 
        where T:IPostscriptComposite =>
        dict.With(key, PostscriptValueFactory.Create(value));
}