using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.InterpreterState;

internal partial class DefaultErrorProc:BuiltInFunction
{
    [FromConstructor] private readonly string errorName;
    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        var errors = engine.ErrorData;
        errors.Put("newerror"u8, true);
        errors.Put("errorname"u8, errorName);
        errors.Put("command"u8, engine.OperandStack.Pop());
        errors.Put("ostack"u8, 
            PostscriptValueFactory.CreateArray(engine.OperandStack.DuplicateToArray()) );
        errors.Put("estack"u8,
            PostscriptValueFactory.CreateArray(engine.ExecutionStack.StackTrace()) );
        errors.Put("dstack"u8, 
            PostscriptValueFactory.CreateArray(engine.DictionaryStack.DumpTrace()) );
        engine.ExecutionStack.HandleStop();
    }
}