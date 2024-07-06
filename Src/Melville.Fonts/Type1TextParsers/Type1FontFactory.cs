using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Fonts.Type1TextParsers;

internal class Type1FontFactory : BuiltInFunction
{
    public Type1GenericFont? Font { get; private set; }

    public IReadOnlyList<IGenericFont> Result =>
        Font is null ? Array.Empty<IGenericFont>() : Font;

    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        var top = engine.OperandStack.Pop();
        CancelPostscriptExecution(engine);
        Font = ExtractFont(top.Get<IPostscriptDictionary>());
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