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
    /// <param name="engine">The engine that to execute the value upon </param>
    /// <param name="value">The value to execute</param>
    void Execute(PostscriptEngine engine, in PostscriptValue value);

    /// <summary>
    /// Accept the value from a parser into the engine.  This typically means executing the token
    /// however, executable arrays have different behavior when they are parsed vs loaded and
    /// this allows that to be different.
    /// </summary>
    /// <param name="engine">The engine that to execute the value upon </param>
    /// <param name="value">The value to execute</param>
    void AcceptParsedToken(PostscriptEngine engine, in PostscriptValue value) =>
        Execute(engine, value);
}