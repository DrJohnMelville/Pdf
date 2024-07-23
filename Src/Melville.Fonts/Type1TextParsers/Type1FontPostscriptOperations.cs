using Melville.Postscript.Interpreter.FunctionLibrary;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Fonts.Type1TextParsers;

internal static partial class Type1FontPostscriptOperations
{
    [PostscriptMethod("definefont")]
    public static void DefineFont(IPostscriptDictionary dict, PostscriptEngine engine)
    {
        engine.Tag = ExtractFont(dict);
        CancelPostscriptExecution(engine);
    }

    private static void CancelPostscriptExecution(PostscriptEngine engine)
    {
        // once the font is defined, the rest of the program is just cleaning
        // up the postscript environment that I do not need because I make a new
        // postscript interpreter every time anyway.  There is some complexity around
        // exiting the eexec section that I do not need to worry about because I just
        // kill the postscript interpreter as soon as it hands me the font dict.
        engine.ExecutionStack.Clear();
    }

    private static Type1GenericFont ExtractFont(IPostscriptDictionary dict)
    {
        return new Type1FontExtractor(dict).Extract();
    }

}