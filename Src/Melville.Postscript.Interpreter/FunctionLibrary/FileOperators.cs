using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class FileOperators
{
    [PostscriptMethod("currentfile")]
    private static PostscriptValue CurrentFile(PostscriptEngine engine) =>
        PostscriptValueFactory.Create(engine.TokenSource?.CodeSource ?? 
                                      throw new PostscriptException("Does not have a CodeSource in a currentfile operation"));

    [PostscriptMethod("closefile")]
    private static void CloseFile(ParsingBuffer file)
    {
        // right now the only file is the current stream so closefile is a no-op that
        // only has to honor the stack signature of the closefile operation.
    }

    [PostscriptMethod("readstring")]
    private static async ValueTask ReadStringAsync(
        ParsingBuffer file, PostscriptLongString str, OperandStack stack)
    {
        await file.TryEnsureBytesAsync(str.Length + 1).CA();
        
        if (file.Peek() is [var possWs, ..] &&
            CharacterClassifier.IsWhitespace(possWs)) file.Advance(1);

        var len = Math.Min(file.CurrentLength, str.Length);
        file.Peek(len).CopyTo(str.GetBytes(default, []));
        file.Advance(len);
        PushReadStringResults(stack, str, len);
    }

    private static void PushReadStringResults(
        OperandStack stack, PostscriptLongString str, int outputLen)
    {
        // str.substring allocates, so we only want to call it if needed for correctness.
        if (outputLen >= str.Length)
            PushReadStringResult(stack, str, true);
        else
            PushReadStringResult(stack, str.Substring(0,outputLen), false);
    }

    private static void PushReadStringResult(
        OperandStack stack, PostscriptLongString str, bool wholeString)
    {
        stack.Push(new PostscriptValue(str, str.StringKind.DefaultAction, default));
        stack.Push(PostscriptValueFactory.Create(wholeString));
    }
}