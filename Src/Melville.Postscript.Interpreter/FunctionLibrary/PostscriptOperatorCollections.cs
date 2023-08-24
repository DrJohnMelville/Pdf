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
    public static IPostscriptDictionary WithcConversionOperators(this IPostscriptDictionary engine) => engine
        .With("cvx"u8, PostscriptOperators.MakeExecutable)
        .With("cvlit"u8, PostscriptOperators.MakeLitreral)
        .With("xcheck"u8, PostscriptOperators.IsExecutable)
        .With("executeonly"u8, PostscriptOperators.Nop)
        .With("readonly"u8, PostscriptOperators.Nop)
        .With("noaccess"u8, PostscriptOperators.Nop)
        .With("rcheck"u8, PostscriptOperators.FakeAccessCheck)
        .With("wcheck"u8, PostscriptOperators.FakeAccessCheck)
        .With("cvi"u8, PostscriptOperators.ConvertToInt)
        .With("cvr"u8, PostscriptOperators.ConvertToDouble)
        .With("cvn"u8, PostscriptOperators.ConvertToName)
        .With("cvs"u8, PostscriptOperators.ConvertToString)
        .With("cvrs"u8, PostscriptOperators.ConvertToRadixString);

    /// <summary>
    /// Implement the control operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithcControlOperators(this IPostscriptDictionary engine) => engine
       .With("exec"u8, PostscriptOperators.Execute)
       .With("if"u8, PostscriptOperators.If)
       .With("ifelse"u8, PostscriptOperators.IfElse)
       .With("for"u8, PostscriptOperators.For)
       .With("repeat"u8, PostscriptOperators.Repeat)
       .With("exit"u8, PostscriptOperators.Exit)
       .With("loop"u8, PostscriptOperators.Loop)
       .With("forall"u8, PostscriptOperators.ForAll)
       .With("stopped"u8, PostscriptOperators.StopRegion)
       .With("stop"u8, PostscriptOperators.Stop)
       .With("countexecstack"u8, PostscriptOperators.CountExecutionStack)
       .With("execstack"u8, PostscriptOperators.ExecStack)
       .With("quit"u8, PostscriptOperators.Quit)
       .With("start"u8, PostscriptOperators.Start);

    /// <summary>
    /// Implement the Relational operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithRelationalOperators(
        this IPostscriptDictionary engine) => engine
        .With("eq"u8, PostscriptOperators.OpEqual)
        .With("ne"u8, PostscriptOperators.OpNotEqual)
        .With("not"u8, PostscriptOperators.Not)
        .With("ge"u8, PostscriptOperators.GreaterThanOrEqual)
        .With("gt"u8, PostscriptOperators.GreaterThan)
        .With("le"u8, PostscriptOperators.LessThanOrEqual)
        .With("lt"u8, PostscriptOperators.LessThan)
        .With("and"u8, PostscriptOperators.And)
        .With("or"u8, PostscriptOperators.Or)
        .With("xor"u8, PostscriptOperators.Xor)
        .With("bitshift"u8, PostscriptOperators.BitShift);

    /// <summary>
    /// Implement the Relational operators in section 8.1
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithDictionaryOperators(
        this IPostscriptDictionary engine) => engine
        .With("dict"u8, PostscriptOperators.CreateDictionary)
        .With("maxlength"u8, PostscriptOperators.DictCapacity)
        .With("<<"u8, PostscriptValueFactory.CreateMark())
        .With(">>"u8, PostscriptOperators.DictionaryFromStack)
        .With("def"u8, PostscriptOperators.DefineInTopDict)
        .With("load"u8, PostscriptOperators.LookupInDictStack)
        .With("begin"u8, PostscriptOperators.PushDictionaryStack)
        .With("end"u8, PostscriptOperators.PopDictionaryStack)
        .With("store"u8, PostscriptOperators.DictionaryStore)
        .With("known"u8, PostscriptOperators.DictionaryKnown)
        .With("undef"u8, PostscriptOperators.Undefine)
        .With("where"u8, PostscriptOperators.Where)
        .With("currentdict"u8, PostscriptOperators.CurrentDictionary)
        .With("userdict"u8, PostscriptOperators.UserDict)
        .With("globaldict"u8, PostscriptOperators.GlobalDict)
        .With("systemdict"u8, PostscriptOperators.SystemDict)
        .With("countdictstack"u8, PostscriptOperators.CountDictStack)
        .With("dictstack"u8, PostscriptOperators.DictStack)
        .With("cleardictstack"u8, PostscriptOperators.ClearDictStack);

    /// <summary>
    /// Implement the uniquely string operators in section 8.1
    /// Many string operators are actually composite or array operators that
    /// just work for strings.
    /// </summary>
    /// <param name="engine">The postscript engine to add definitions too.</param>
    /// <returns>The engine passed in the first parameter</returns>
    public static IPostscriptDictionary WithStringOperators(
        this IPostscriptDictionary engine) => engine
        .With("anchorsearch"u8, PostscriptOperators.AnchorSearch)
        .With("search"u8, PostscriptOperators.Search)
        .With("token"u8, PostscriptOperators.Token)
    ;

}