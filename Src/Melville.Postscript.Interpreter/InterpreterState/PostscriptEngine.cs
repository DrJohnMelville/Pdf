using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    public OperandStack OperandStack { get; } = new();
    /// <summary>
    /// The current dictionary stack from which names are resolved
    /// </summary>
    public DictionaryStack DictionaryStack { get; } = new();
    /// <summary>
    /// The current callstack
    /// </summary>
    public ExecutionStack ExecutionStack { get; } = new();

    private int deferredExecutionCount = 0;

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
        CreateStandardDictionarystack();
        CreateSecondaryStandardDictionaries();
    }

    private void CreateStandardDictionarystack()
    {
        DictionaryStack.Add(new PostscriptLongDictionary());
        DictionaryStack.Add(new PostscriptLongDictionary());
        DictionaryStack.Add(new PostscriptLongDictionary());
    }

    private void CreateSecondaryStandardDictionaries()
    {
#warning consider actually implementing the postscript error handling routines.
        UserDict.Put("errordict", new PostscriptShortDictionary(0).AsPostscriptValue());
        UserDict.Put("$error", new PostscriptShortDictionary(0).AsPostscriptValue());
        UserDict.Put("statusdict", new PostscriptShortDictionary(0).AsPostscriptValue());
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

    internal ValueTask ExecuteAsync(AsynchronousTokenizer tokens)
    {
        Debug.Assert(ExecutionStack.Count == 0);
        ExecutionStack.Push(new(tokens.GetAsyncEnumerator()), "Async Parser"u8);
        return MainExecutionLoopAsync();
    }

    private async ValueTask MainExecutionLoopAsync()
    {
        while (await ExecutionStack.NextInstructionAsync() is {} token) 
            AcceptToken(token);
    }


    internal void Execute(string code) => 
        Execute(Encoding.ASCII.GetBytes(code));
    internal void Execute(in Memory<byte> code) => Execute(SynchronousTokenizer.Tokenize(code));

    internal void Execute(IEnumerable<PostscriptValue> tokens)
    {
        Debug.Assert(ExecutionStack.Count == 0);
        ExecutionStack.Push(new(tokens.GetEnumerator()), "Synchronous source");
        MainExecutionLoop();
    }

    private void MainExecutionLoop()
    {
        while (ExecutionStack.NextInstruction(out var token)) AcceptToken(token);
    }

    private static readonly PostscriptValue OpenProc =
        PostscriptValueFactory.CreateString("{"u8, StringKind.Name);
    private static readonly PostscriptValue CloseProc =
        PostscriptValueFactory.CreateString("}"u8, StringKind.Name);
    private void AcceptToken(in PostscriptValue current)
    {
        CheckForCloseProcToken(current);
        PushOrExecuteToken(current);
        CheckForOpenProcToken(current);
    }

    private void PushOrExecuteToken(PostscriptValue current)
    {
        if (deferredExecutionCount > 0)
            OperandStack.Push(current);
        else
            current.ExecutionStrategy.AcceptParsedToken(this, current);
    }

    private void CheckForCloseProcToken(PostscriptValue current)
    {
        if (current.Equals(CloseProc)) deferredExecutionCount--;
    }

    private void CheckForOpenProcToken(PostscriptValue current)
    {
        if (current.Equals(OpenProc)) deferredExecutionCount++;
    }
}
