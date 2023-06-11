using System.Security.Principal;
using Melville.Postscript.Interpreter.InterpreterState;

namespace Melville.Postscript.Interpreter.Values.Execution;

/// <summary>
/// This interface represents a function that can be executed in the engine
/// </summary>
public interface IExecutePostscript
{
    /// <summary>
    /// Execute the given command on a Postscript engine
    /// </summary>
    /// <param name="engine">The engine that is </param>
    /// <param name="value"></param>
    void Execute(PostscriptEngine engine, in PostscriptValue value);
}