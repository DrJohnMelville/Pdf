using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

/// <summary>
/// This facade class loads various groups of 
/// </summary>
public static class PostscriptOperatorCollections
{
    /// <summary>
    /// Add an external function to the system dictionary for a Postscript Engine
    /// </summary>
    /// <param name="engine">The engine to add </param>
    /// <param name="name"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static PostscriptEngine WithSystemFunction(
        this PostscriptEngine engine, string name, IExternalFunction func)
    {
        engine.SystemDict.Add(name, PostscriptValueFactory.Create(func));
        return engine;
    }

    /// <summary>
    /// Define true, false, and null tokens
    /// </summary>
    /// <param name="engine">The postscript to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithSystemTokens(this PostscriptEngine engine) => engine
        .WithSystemFunction("true", PostscriptOperators.PushTrue)
        .WithSystemFunction("false", PostscriptOperators.PushFalse)
        .WithSystemFunction("null", PostscriptOperators.PushNull);


    /// <summary>
    /// Implement the stack operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithStackOperators(this PostscriptEngine engine) => engine
        .WithSystemFunction("pop", PostscriptOperators.Pop)
        .WithSystemFunction("exch", PostscriptOperators.Exchange)
        .WithSystemFunction("dup", PostscriptOperators.Duplicate)
        .WithSystemFunction("copy", PostscriptOperators.CopyTop)
        .WithSystemFunction("index", PostscriptOperators.IndexOperation)
        .WithSystemFunction("roll", PostscriptOperators.Roll)
        .WithSystemFunction("clear", PostscriptOperators.ClearStack)
        .WithSystemFunction("count", PostscriptOperators.CountStack)
        .WithSystemFunction("mark", PostscriptOperators.PlaceMark)
        .WithSystemFunction("cleartomark", PostscriptOperators.ClearToMark)
        .WithSystemFunction("counttomark", PostscriptOperators.CountToMark);
}