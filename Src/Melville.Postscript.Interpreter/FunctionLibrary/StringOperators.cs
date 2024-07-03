using System;
using System.Buffers;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class StringOperators
{
    [PostscriptMethod("anchorsearch")]
    private static void AnchorSearch(OperandStack stack, PostscriptValue seek) =>
        stack.Peek().Get<PostscriptLongString>().DoAnchorSearch(stack, seek);

    [PostscriptMethod("search")]
    private static void Search(OperandStack stack, PostscriptValue seek) =>
        stack.Peek().Get<PostscriptLongString>().DoSearch(stack, seek);

    [PostscriptMethod("token")]
    private static void Tokenizers(IPostscriptTokenSource source, OperandStack stack) =>
        source.GetToken(stack);

    [PostscriptMethod("string")]
    private static PostscriptValue String(long length) => 
        PostscriptValueFactory.CreateLongString(new byte[(int)length], StringKind.String);
    
    [PostscriptMethod("currentfile")]
    private static PostscriptValue CurrentFile(PostscriptEngine engine) =>
        PostscriptValueFactory.Create(engine.TokenSource?.CodeSource ?? 
            throw new PostscriptException("Does not have a CodeSource in a currentfile operation"));

    [PostscriptMethod("readstring")]
    private static void ReadString(
        IByteSourceWithGlobalPosition file, PostscriptLongString str, OperandStack stack)
    {
        var result = file.ReadAtLeast(str.Length+1);
        if (result.Buffer.Length < 1)
        {
            stack.Push(PostscriptValueFactory.CreateString("", str.StringKind));
            stack.Push(PostscriptValueFactory.Create(false));
            file.AdvanceTo(result.Buffer.End);
            return;
        }

        var start = CharacterClassifier.IsWhitespace(result.Buffer.FirstSpan[0]) ? 1 : 0;
        var outputLen = (int)Math.Min(str.Length, result.Buffer.Length-start);
        var destination = str.GetBytes(default,[])[..outputLen];
        result.Buffer.Slice(start, outputLen).CopyTo(destination);

        if (outputLen >= str.Length)
        {
            stack.Push(new PostscriptValue(str, str.StringKind.DefaultAction, default));
            stack.Push(PostscriptValueFactory.Create(true));
        }
        else
        {
            stack.Push(new PostscriptValue(str.Substring(0, outputLen), 
                str.StringKind.DefaultAction, default));
            stack.Push(PostscriptValueFactory.Create(false));
        }
        file.AdvanceTo(result.Buffer.GetPosition(start+outputLen));
    }
}