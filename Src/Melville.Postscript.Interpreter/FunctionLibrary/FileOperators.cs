using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class FileOperators
{
    [PostscriptMethod("currentfile")]
    private static PostscriptValue CurrentFile(PostscriptEngine engine) =>
        PostscriptValueFactory.Create(engine.TokenSource?.CodeSource ?? 
                                      throw new PostscriptException("Does not have a CodeSource in a currentfile operation"));

    [PostscriptMethod("closefile")]
    private static void CloseFile(IByteSourceWithGlobalPosition file)
    {
        // right now the only file is the current stream so closefile is a no-op that
        // only has to honor the stack signature of the closefile operation.
    }

    [PostscriptMethod("readstring")]
    private static async ValueTask ReadStringAsync(
        IByteSourceWithGlobalPosition file, PostscriptLongString str, OperandStack stack)
    {
        var result = await file.ReadAtLeastAsync(str.Length+1);
        if (TryHandleEmptyRead(file, str, stack, result)) return;
        var start = TrySkipWhiteSpace(result);
        var outputLen = (int)Math.Min(str.Length, result.Buffer.Length-start);
        ReadStringSync(str, result.Buffer.Slice(start, outputLen));

        PushReadStringResults(stack, str, outputLen);
        file.AdvanceTo(result.Buffer.GetPosition(start+outputLen));
    }

    private static void ReadStringSync(
        PostscriptLongString str, ReadOnlySequence<byte> trimmedResult)
    {
        var destination = str.GetBytes(default,[])[..(int)trimmedResult.Length];
        trimmedResult.CopyTo(destination);
    }

    private static void PushReadStringResults(
        OperandStack stack, PostscriptLongString str, int outputLen)
    {
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

    private static int TrySkipWhiteSpace(ReadResult result) => 
        CharacterClassifier.IsWhitespace(result.Buffer.FirstSpan[0]) ? 1 : 0;

    private static bool TryHandleEmptyRead(IByteSourceWithGlobalPosition file, PostscriptLongString str, OperandStack stack,
        ReadResult result)
    {
        if (result.Buffer.Length < 1)
        {
            stack.Push(PostscriptValueFactory.CreateString("", str.StringKind));
            stack.Push(PostscriptValueFactory.Create(false));
            file.AdvanceTo(result.Buffer.End);
            return true;
        }

        return false;
    }
}