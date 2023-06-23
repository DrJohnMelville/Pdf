using System;
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
    /// <param name="engine">The engine to add to</param>
    /// <param name="name">Name of the item</param>
    /// <param name="func">The function to put</param>
    /// <returns>The passed in engine</returns>
    public static PostscriptEngine WithSystemFunction(
        this PostscriptEngine engine, ReadOnlySpan<byte> name, IExternalFunction func)
    {
        engine.SystemDict.Put(name, PostscriptValueFactory.Create(func));
        return engine;
    }
    /// <summary>
    /// Add an external function to the system dictionary for a Postscript Engine
    /// </summary>
    /// <param name="engine">The engine to add to</param>
    /// <param name="name">Name of the item</param>
    /// <param name="func">The function to put</param>
    /// <returns>The passed in engine</returns>
    public static PostscriptEngine WithSystemConstant(
        this PostscriptEngine engine, ReadOnlySpan<byte> name, in PostscriptValue value)
    {
        engine.SystemDict.Put(name, value);
        return engine;
    }

    public static PostscriptEngine WithBaseLanguage(this PostscriptEngine engine) => engine
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
    public static PostscriptEngine WithSystemTokens(this PostscriptEngine engine) => engine
    .WithSystemConstant("true"u8, true)
    .WithSystemConstant("false"u8, false)
    .WithSystemConstant("null"u8, PostscriptValueFactory.CreateNull());
    
    /// <summary>
    /// Implement the stack operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithStackOperators(this PostscriptEngine engine) => engine
        .WithSystemFunction("pop"u8, PostscriptOperators.Pop)
        .WithSystemFunction("exch"u8, PostscriptOperators.Exchange)
        .WithSystemFunction("dup"u8, PostscriptOperators.Duplicate)
        .WithSystemFunction("copy"u8, PostscriptOperators.CopyTop)
        .WithSystemFunction("index"u8, PostscriptOperators.IndexOperation)
        .WithSystemFunction("roll"u8, PostscriptOperators.Roll)
        .WithSystemFunction("clear"u8, PostscriptOperators.ClearStack)
        .WithSystemFunction("count"u8, PostscriptOperators.CountStack)
        .WithSystemConstant("mark"u8, PostscriptValueFactory.CreateMark())
        .WithSystemFunction("cleartomark"u8, PostscriptOperators.ClearToMark)
        .WithSystemFunction("counttomark"u8, PostscriptOperators.CountToMark);

    /// <summary>
    /// Implement the stack operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithMathOperators(this PostscriptEngine engine) => engine
        .WithSystemFunction("add"u8, PostscriptOperators.Add)
        .WithSystemFunction("div"u8, PostscriptOperators.RealDivide)
        .WithSystemFunction("idiv"u8, PostscriptOperators.IntegerDivide)
        .WithSystemFunction("mod"u8, PostscriptOperators.Modulo)
        .WithSystemFunction("mul"u8, PostscriptOperators.Multiply)
        .WithSystemFunction("sub"u8, PostscriptOperators.Subtract)
        .WithSystemFunction("abs"u8, PostscriptOperators.AbsoluteValue)
        .WithSystemFunction("neg"u8, PostscriptOperators.Negative)
        .WithSystemFunction("ceiling"u8, PostscriptOperators.Ceiling)
        .WithSystemFunction("floor"u8, PostscriptOperators.Floor)
        .WithSystemFunction("round"u8, PostscriptOperators.Round)
        .WithSystemFunction("truncate"u8, PostscriptOperators.Truncate)
        .WithSystemFunction("sqrt"u8, PostscriptOperators.SquareRoot)
        .WithSystemFunction("atan"u8, PostscriptOperators.ArcTangent)
        .WithSystemFunction("sin"u8, PostscriptOperators.Sine)
        .WithSystemFunction("cos"u8, PostscriptOperators.Cosine)
        .WithSystemFunction("exp"u8, PostscriptOperators.RaiseToPower)
        .WithSystemFunction("log"u8, PostscriptOperators.Log10)
        .WithSystemFunction("ln"u8, PostscriptOperators.NaturalLog)
        .WithSystemFunction("srand"u8, PostscriptOperators.SetRandomSeed)
        .WithSystemFunction("rrand"u8, PostscriptOperators.GetRandomSeed)
        .WithSystemFunction("rand"u8, PostscriptOperators.GetNextRandom);

    /// <summary>
    /// Implement the array operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithArrayOperators(this PostscriptEngine engine) => engine
        .WithSystemFunction("array"u8, PostscriptOperators.EmptyArray)
        .WithSystemConstant("["u8, PostscriptValueFactory.CreateMark())
        .WithSystemFunction("]"u8, PostscriptOperators.ArrayFromStack)
        .WithSystemConstant("{"u8, PostscriptValueFactory.CreateMark())
        .WithSystemFunction("}"u8, PostscriptOperators.ProcFromStack)
        .WithSystemFunction("length"u8, PostscriptOperators.CompositeLength)
        .WithSystemFunction("get"u8, PostscriptOperators.CompositeGet)
        .WithSystemFunction("put"u8, PostscriptOperators.CompositePut)
        .WithSystemFunction("getinterval"u8, PostscriptOperators.GetInterval)
        .WithSystemFunction("putinterval"u8, PostscriptOperators.PutInterval)
        .WithSystemFunction("astore"u8, PostscriptOperators.AStore)
        .WithSystemFunction("aload"u8, PostscriptOperators.ALoad)
        .WithSystemFunction("currentpacking"u8, PostscriptOperators.CurrentPacking)
        .WithSystemFunction("setpacking"u8, PostscriptOperators.SetPacking)
        .WithSystemFunction("packedarray"u8, PostscriptOperators.PackedArray);

    /// <summary>
    /// Implement the type conversion operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithcConversionOperators(this PostscriptEngine engine) => engine
        .WithSystemFunction("cvx"u8, PostscriptOperators.MakeExecutable)
        .WithSystemFunction("cvlit"u8, PostscriptOperators.MakeLitreral)
        .WithSystemFunction("xcheck"u8, PostscriptOperators.IsExecutable)
        .WithSystemFunction("executeonly"u8, PostscriptOperators.Nop)
        .WithSystemFunction("readonly"u8, PostscriptOperators.Nop)
        .WithSystemFunction("noaccess"u8, PostscriptOperators.Nop)
        .WithSystemFunction("rcheck"u8, PostscriptOperators.FakeAccessCheck)
        .WithSystemFunction("wcheck"u8, PostscriptOperators.FakeAccessCheck)
        .WithSystemFunction("cvi"u8, PostscriptOperators.ConvertToInt)
        .WithSystemFunction("cvr"u8, PostscriptOperators.ConvertToDouble)
        .WithSystemFunction("cvn"u8, PostscriptOperators.ConvertToName)
        .WithSystemFunction("cvs"u8, PostscriptOperators.ConvertToString)
        .WithSystemFunction("cvrs"u8, PostscriptOperators.ConvertToRadixString);

    /// <summary>
    /// Implement the control operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithcControlOperators(this PostscriptEngine engine) => engine
       .WithSystemFunction("exec"u8, PostscriptOperators.Execute)
       .WithSystemFunction("if"u8, PostscriptOperators.If)
       .WithSystemFunction("ifelse"u8, PostscriptOperators.IfElse)
       .WithSystemFunction("for"u8, PostscriptOperators.For)
       .WithSystemFunction("repeat"u8, PostscriptOperators.Repeat)
       .WithSystemFunction("exit"u8, PostscriptOperators.Exit)
       .WithSystemFunction("loop"u8, PostscriptOperators.Loop)
       .WithSystemFunction("forall"u8, PostscriptOperators.ForAll)
       .WithSystemFunction("stopped"u8, PostscriptOperators.StopRegion)
       .WithSystemFunction("stop"u8, PostscriptOperators.Stop)
       .WithSystemFunction("countexecstack"u8, PostscriptOperators.CountExecutionStack)
       .WithSystemFunction("execstack"u8, PostscriptOperators.ExecStack)
       .WithSystemFunction("quit"u8, PostscriptOperators.Quit)
       .WithSystemFunction("start"u8, PostscriptOperators.Start);

    /// <summary>
    /// Implement the Relational operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithRelationalOperators(
        this PostscriptEngine engine) => engine
        .WithSystemFunction("eq"u8, PostscriptOperators.OpEqual)
        .WithSystemFunction("ne"u8, PostscriptOperators.OpNotEqual)
        .WithSystemFunction("not"u8, PostscriptOperators.Not)
        .WithSystemFunction("ge"u8, PostscriptOperators.GreaterThanOrEqual)
        .WithSystemFunction("gt"u8, PostscriptOperators.GreaterThan)
        .WithSystemFunction("le"u8, PostscriptOperators.LessThanOrEqual)
        .WithSystemFunction("lt"u8, PostscriptOperators.LessThan)
        .WithSystemFunction("and"u8, PostscriptOperators.And)
        .WithSystemFunction("or"u8, PostscriptOperators.Or)
        .WithSystemFunction("xor"u8, PostscriptOperators.Xor)
        .WithSystemFunction("bitshift"u8, PostscriptOperators.BitShift);

    /// <summary>
    /// Implement the Relational operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithDictionaryOperators(
        this PostscriptEngine engine) => engine
        .WithSystemFunction("dict"u8, PostscriptOperators.CreateDictionary)
        .WithSystemFunction("maxlength"u8, PostscriptOperators.DictCapacity)
        .WithSystemConstant("<<"u8, PostscriptValueFactory.CreateMark())
        .WithSystemFunction(">>"u8, PostscriptOperators.DictionaryFromStack)
        .WithSystemFunction("def"u8, PostscriptOperators.DefineInTopDict)
        .WithSystemFunction("load"u8, PostscriptOperators.LookupInDictStack)
        .WithSystemFunction("begin"u8, PostscriptOperators.PushDictionaryStack)
        .WithSystemFunction("end"u8, PostscriptOperators.PopDictionaryStack)
        .WithSystemFunction("store"u8, PostscriptOperators.DictionaryStore)
        .WithSystemFunction("known"u8, PostscriptOperators.DictionaryKnown)
        .WithSystemFunction("undef"u8, PostscriptOperators.Undefine)
        .WithSystemFunction("where"u8, PostscriptOperators.Where)
        .WithSystemFunction("currentdict"u8, PostscriptOperators.CurrentDictionary)
        .WithSystemConstant("userdict"u8, engine.UserDict.AsPostscriptValue())
        .WithSystemConstant("globaldict"u8, engine.GlobalDict.AsPostscriptValue())
        .WithSystemConstant("systemdict"u8, engine.SystemDict.AsPostscriptValue())
        .WithSystemFunction("countdictstack"u8, PostscriptOperators.CountDictStack)
        .WithSystemFunction("dictstack"u8, PostscriptOperators.DictStack)
        .WithSystemFunction("cleardictstack"u8, PostscriptOperators.ClearDictStack);

    /// <summary>
    /// Implement the uniquely string operators in section 8.1
    /// Many string operators are actually composite or array operators that
    /// just work for strings.
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static PostscriptEngine WithStringOperators(
        this PostscriptEngine engine) => engine
        .WithSystemFunction("anchorsearch"u8, PostscriptOperators.AnchorSearch)
        .WithSystemFunction("search"u8, PostscriptOperators.Search)
    ;

}