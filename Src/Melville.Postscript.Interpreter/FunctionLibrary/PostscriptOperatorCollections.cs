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
    /// <param name="engine">The postscript engine to add definitions too.</param>
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

    /// <summary>
    /// Implement the stack operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithMathOperators(this PostscriptEngine engine) => engine
        .WithSystemFunction("add", PostscriptOperators.Add)
        .WithSystemFunction("div", PostscriptOperators.RealDivide)
        .WithSystemFunction("idiv", PostscriptOperators.IntegerDivide)
        .WithSystemFunction("mod", PostscriptOperators.Modulo)
        .WithSystemFunction("mul", PostscriptOperators.Multiply)
        .WithSystemFunction("sub", PostscriptOperators.Subtract)
        .WithSystemFunction("abs", PostscriptOperators.AbsoluteValue)
        .WithSystemFunction("neg", PostscriptOperators.Negative)
        .WithSystemFunction("ceiling", PostscriptOperators.Ceiling)
        .WithSystemFunction("floor", PostscriptOperators.Floor)
        .WithSystemFunction("round", PostscriptOperators.Round)
        .WithSystemFunction("truncate", PostscriptOperators.Truncate)
        .WithSystemFunction("sqrt", PostscriptOperators.SquareRoot)
        .WithSystemFunction("atan", PostscriptOperators.ArcTangent)
        .WithSystemFunction("sin", PostscriptOperators.Sine)
        .WithSystemFunction("cos", PostscriptOperators.Cosine)
        .WithSystemFunction("exp", PostscriptOperators.RaiseToPower)
        .WithSystemFunction("log", PostscriptOperators.Log10)
        .WithSystemFunction("ln", PostscriptOperators.NaturalLog)
        .WithSystemFunction("srand", PostscriptOperators.SetRandomSeed)
        .WithSystemFunction("rrand", PostscriptOperators.GetRandomSeed)
        .WithSystemFunction("rand", PostscriptOperators.GetNextRandom)
    ;

}