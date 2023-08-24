using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class SystemTokens 
{
    [PostscriptMethod("true")]
    private static PostscriptValue True() => true;
    [PostscriptMethod("false")]
    private static PostscriptValue False() => false;
    [PostscriptMethod("null")]
    private static PostscriptValue Null() => PostscriptValueFactory.CreateNull();
}