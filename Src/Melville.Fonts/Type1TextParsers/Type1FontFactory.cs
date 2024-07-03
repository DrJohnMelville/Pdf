using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Fonts.Type1TextParsers;

public class Type1FontFactory : BuiltInFunction
{
    public Type1GenericFont? Font { get; private set; }

    public IReadOnlyList<IGenericFont> Result =>
        Font is null ? [] : Font;

    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        var top = engine.OperandStack.Pop();
        engine.OperandStack.Pop();
        engine.OperandStack.Push(top);
        Font = new Type1GenericFont(top.Get<IPostscriptDictionary>());
    }
}