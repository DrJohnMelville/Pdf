using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
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

    /// <summary>
    /// Execute a postscript program.
    /// </summary>
    /// <param name="code">The program to execute</param>
    public ValueTask ExecuteAsync(string code) =>
        ExecuteAsync(Encoding.ASCII.GetBytes(code));
    /// <summary>
    /// Execute a postscript program.
    /// </summary>
    /// <param name="code">The program to execute</param>
    public ValueTask ExecuteAsync(in Memory<byte> code) => ExecuteAsync(
        new Tokenizer(code));
    /// <summary>
    /// Execute a postscript program.
    /// </summary>
    /// <param name="code">The program to execute</param>
    public ValueTask ExecuteAsync(Stream code) =>
        ExecuteAsync(new Tokenizer(code));

    /// <summary>
    /// Execute a postscript program.
    /// </summary>
    /// <param name="tokens">A tokenizer containing the program to execute.</param>
    public async ValueTask ExecuteAsync(ITokenSource tokens)
    {
        Debug.Assert(ExecutionStack.Count == 0);
        await ExecutionStack.PushAsync(new(tokens.TokensAsync().GetAsyncEnumerator()), 
            "Async Parser"u8);
        await MainExecutionLoopAsync();
    }

    private async ValueTask MainExecutionLoopAsync()
    {
        while (await ExecutionStack.NextInstructionAsync() is {} token)
        {
            if (ShouldExecuteToken(token))
                await token.ExecutionStrategy.AcceptParsedTokenAsync(this, token);
            CheckForOpenProcToken(token);
        }
    }


    /// <summary>
    /// Execute a postscript program.
    /// </summary>
    /// <param name="code">The program to execute</param>
    public void Execute(string code) => 
        Execute(Encoding.ASCII.GetBytes(code));
    /// <summary>
    /// Execute a postscript program.
    /// </summary>
    /// <param name="code">The program to execute</param>
    public void Execute(in Memory<byte> code) => Execute(
        new Tokenizer(code));

    /// <summary>
    /// Execute a postscript program.
    /// </summary>
    /// <param name="tokens">A token source representing the program.</param>
    public void Execute(ITokenSource tokens)
    {
        ExecutionStack.Push(new(tokens.Tokens().GetEnumerator()), "Synchronous CodeSource");
        MainExecutionLoop();
    }

    private void MainExecutionLoop()
    {
        while (ExecutionStack.NextInstruction(out var token))
        {
            if (ShouldExecuteToken(token))
                token.ExecutionStrategy.AcceptParsedToken(this, token);
            CheckForOpenProcToken(token);
        }
    }

    private bool ShouldExecuteToken(in PostscriptValue token)
    {
        CheckForCloseProcToken(token);
        if (deferredExecutionCount <= 0 ) return true;
        OperandStack.Push(token);
        return false;
    }

    private static readonly PostscriptValue OpenProc =
        PostscriptValueFactory.CreateString("{"u8, StringKind.Name);
    private static readonly PostscriptValue CloseProc =
        PostscriptValueFactory.CreateString("}"u8, StringKind.Name);

    private void CheckForCloseProcToken(in PostscriptValue current)
    {
        if (current.Equals(CloseProc)) deferredExecutionCount--;
    }

    private void CheckForOpenProcToken(in PostscriptValue current)
    {
        if (current.Equals(OpenProc)) deferredExecutionCount++;
    }
}