using System.Security.Principal;
using System.Threading.Tasks;
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
    /// Execute the given command on a Postscript engine
    /// </summary>
    /// <param name="engine">The engine that to execute the value upon </param>
    /// <param name="value">The value to execute</param>
    ValueTask ExecuteAsync(PostscriptEngine engine, in PostscriptValue value)
    {
        Execute(engine, value);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Accept the value from a parser into the engine.  This typically means executing the token
    /// however, executable arrays have different behavior when they are parsed vs loaded and
    /// this allows that to be different.
    /// </summary>
    /// <param name="engine">The engine that to execute the value upon </param>
    /// <param name="value">The value to execute</param>
    void AcceptParsedToken(PostscriptEngine engine, in PostscriptValue value) =>
        Execute(engine, value);

    /// <summary>
    /// Accept the value from a parser into the engine.  This typically means executing the token
    /// however, executable arrays have different behavior when they are parsed vs loaded and
    /// this allows that to be different.
    /// </summary>
    /// <param name="engine">The engine that to execute the value upon </param>
    /// <param name="value">The value to execute</param>
    ValueTask AcceptParsedTokenAsync(PostscriptEngine engine, PostscriptValue value) => 
        ExecuteAsync(engine, value);

    /// <summary>
    /// Wrap the display of the value based on its execution status
    /// </summary>
    /// <param name="text">The text that represents the unadorned value</param>
    /// <returns>The text as it should be displayed</returns>
    string WrapTextDisplay(string text);

    /// <summary>
    /// Determine if this strategy corresponds to an executable object.
    /// This is the value returned from the xcheck operator.
    /// </summary>
    bool IsExecutable { get; }
}