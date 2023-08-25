using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// This facade class loads various groups of 
/// </summary>
public static class PostscriptOperatorCollections
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
    /// Configure a postscript engine with the base postscript language.
    /// </summary>
    /// <param name="engine">The engine to configure.</param>
    /// <returns>Thge engine passed in, once it has been configured/</returns>
    public static IPostscriptDictionary WithBaseLanguage(this IPostscriptDictionary engine) => engine
        .WithSystemTokens()
        .WithStackOperators()
        .WithMathOperators()
        .WithArrayOperators()
        .WithcConversionOperators()
        .WithcControlOperators()
        .WithRelationalOperators()
        .WithDictionaryOperators()
        .WithStringOperators();


    /// <summary>
    /// Define true, false, and null tokens
    /// </summary>
    /// <param name="engine">The postscript to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithSystemTokens(this IPostscriptDictionary engine)
    {
        SystemTokens.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the stack operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithStackOperators(this IPostscriptDictionary engine)
    {
        StackOperators.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the stack operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithMathOperators(this IPostscriptDictionary engine)
    {
        MathOperators.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the array operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithArrayOperators(this IPostscriptDictionary engine)
    {
        ArrayOperators.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the type conversion operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithcConversionOperators(this IPostscriptDictionary engine)
    {
        ConversionOperators.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the control operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithcControlOperators(this IPostscriptDictionary engine)
    {
        ControlOperators.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the Relational operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithRelationalOperators(
        this IPostscriptDictionary engine)
    {
        RelationalAndBitwiseOperators.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the Relational operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithDictionaryOperators(
        this IPostscriptDictionary engine)
    {
        DictionaryOperators.AddOperations(engine);
        return engine;
    }

    /// <summary>
    /// Implement the uniquely string operators in section 8.1
    /// Many string operators are actually composite or array operators that
    /// just work for strings.
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithStringOperators(this IPostscriptDictionary engine)
    {
        StringSearchOperators.AddOperations(engine);
        return engine;
    }
    

}