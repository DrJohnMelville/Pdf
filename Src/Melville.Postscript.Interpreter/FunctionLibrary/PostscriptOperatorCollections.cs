using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// This facade class loads various groups of 
/// </summary>
[MacroItem("SystemTokens")]
[MacroItem("StackOperators")]
[MacroItem("MathOperators")]
[MacroItem("ArrayOperators")]
[MacroItem("ConversionOperators")]
[MacroItem("ControlOperators")]
[MacroItem("RelationalAndBitwiseOperators")]
[MacroItem("DictionaryOperators")]
[MacroItem("StringOperators")]
[MacroItem("ResourceOperators")]
[MacroCode("""
    /// <summary>
    /// Implement the ~0~ in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary With~0~(this IPostscriptDictionary engine) =>
        engine.With(~0~.AddOperations);
        
""")]
public static partial class PostscriptOperatorCollections
{
    /// <summary>
    /// A dictionary containing all the base postscript languae elements.
    /// </summary>
    public static IPostscriptDictionary BaseLanguage() =>
        Empty().WithBaseLanguage();

    /// <summary>
    /// An emptypostscript dictionary to   build postscript collections from.
    /// </summary>
    /// <returns></returns>
    public static IPostscriptDictionary Empty() =>
        new PostscriptLongDictionary();

    /// <summary>
    /// Helper method to chain adding operator collections to a postscriptdictionary
    /// </summary>
    /// <param name="dict">The dictionary to add operations to</param>
    /// <param name="content">Action to add items to the dictionary</param>
    /// <returns>The passed in dictionary after the action is run.</returns>
    public static IPostscriptDictionary With(
        this IPostscriptDictionary dict, Action<IPostscriptDictionary> content)
    {
        content(dict);
        return dict;
    }


    /// <summary>
    /// Configure a postscript engine with the base postscript language.
    /// </summary>
    /// <param name="engine">The engine to configure.</param>
    /// <returns>The engine passed in, once it has been configured/</returns>
    public static IPostscriptDictionary WithBaseLanguage(this IPostscriptDictionary engine) => engine
        .WithSystemTokens()
        .WithStackOperators()
        .WithMathOperators()
        .WithArrayOperators()
        .WithConversionOperators()
        .WithControlOperators()
        .WithRelationalAndBitwiseOperators()
        .WithDictionaryOperators()
        .WithStringOperators()
        .WithResourceOperators();
}