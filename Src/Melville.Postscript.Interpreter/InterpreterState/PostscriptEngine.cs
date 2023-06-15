using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.InterpreterState;

/// <summary>
/// This class represents the state of the postscript interpreter.
/// Postscript operators modify the state to implement their operations.
/// </summary>
public class PostscriptEngine
{
    /// <summary>
    /// This is the current operation stack for the engine;
    /// </summary>
    public PostscriptStack<PostscriptValue> OperandStack { get; } = new(0);
    /// <summary>
    /// The current dictionary stack from which names are resolved
    /// </summary>
    public DictionaryStack DictionaryStack { get; } = new();
    /// <summary>
    /// The current callstack
    /// </summary>
    public PostscriptStack<IAsyncEnumerator<PostscriptValue>> ExecutionStack { get; } = new(0);

    /// <summary>
    /// The Postscript language defines regular arrays, and a more limited but efficient packed
    /// array.  This interpreter does not honor that distinction, and all arrays are just arrays.
    /// However the spec allows programs to set and read the current packing state, so we have to
    /// preserve this value even though it is completely nonfunctional in this implementation.
    /// </summary>
    public bool PackingMode { get; set; }

    private LehmerRandomNumberGenerator random = new();
    internal ref LehmerRandomNumberGenerator Random => ref random;

    /// <summary>
    /// Create a new PostScriptEngine
    /// </summary>
    public PostscriptEngine()
    {
        DictionaryStack.Add(new PostscriptLongDictionary());
        DictionaryStack.Add(new PostscriptLongDictionary());
        DictionaryStack.Add(new PostscriptLongDictionary());
    }

    /// <summary>
    /// The system dictionary for executable names.
    /// </summary>
    public IPostscriptComposite SystemDict => DictionaryStack[0];
    /// <summary>
    /// The global executable dictionary for executable names.
    /// </summary>
    public IPostscriptComposite GlobalDict => DictionaryStack[1];
    /// <summary>
    /// The user dictionary for executable names.
    /// </summary>
    public IPostscriptComposite UserDict => DictionaryStack[2];

    internal ValueTask ExecuteAsync(Tokenizer tokens)
    {
        Debug.Assert(ExecutionStack.Count == 0);
        ExecutionStack.Push(tokens.GetAsyncEnumerator());
        return MainExecutionLoopAsync();
    }

    private async ValueTask MainExecutionLoopAsync()
    {
        while (ExecutionStack.Count > 0)
        {
            var nextInstructionSource = ExecutionStack.Peek();
            if (await nextInstructionSource.MoveNextAsync())
            {
                ExecuteToken(nextInstructionSource.Current);
            }
            else
            {
#pragma warning disable CS4014
                ExecutionStack.Pop();
#pragma warning restore CS4014
            }
        }
    }

    private void ExecuteToken(in PostscriptValue current)
    {
        current.Get<IExecutePostscript>().Execute(this, current);
    }
}
