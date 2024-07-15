using System.Runtime.InteropServices;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Postscript.Interpreter.FunctionLibrary;

internal static partial class DictionaryOperators
{
    [PostscriptMethod("dict")]
    private static PostscriptValue CreateDictionary(int length) =>
        PostscriptValueFactory.CreateSizedDictionary(length);

    [PostscriptMethod("maxlength")]
    private static int DictCapacity(IPostscriptDictionary dict) => dict.MaxLength;

    [PostscriptMethod("<<")]
    private static PostscriptValue BeginLiteralDict() => PostscriptValueFactory.CreateMark();

    [PostscriptMethod(">>")]
    private static PostscriptValue CreateLiteralDict(OperandStack stack) =>
        stack.DictionaryFromMarkedSpan();

    [PostscriptMethod("def")]
    private static void DefineInTopDict(
        in PostscriptValue key, in PostscriptValue value, DictionaryStack dicts) =>
        dicts.Peek().Put(key, value);

    [PostscriptMethod("load")]
    private static PostscriptValue LookUpIn(in PostscriptValue key, DictionaryStack dicts) =>
        dicts.Get(key);

    [PostscriptMethod("begin")]
    private static void PushDictionaryStack(in PostscriptValue newDict, DictionaryStack dicts)=>
        dicts.Push(newDict);

    [PostscriptMethod("end")]
    private static void PopDictionaryStack(DictionaryStack dicts) => dicts.Pop();

    [PostscriptMethod("store")]
    private static void DictionaryStore(
        in PostscriptValue key, in PostscriptValue value, DictionaryStack dicts) =>
        dicts.Store(key, value);

    [PostscriptMethod("known")]
    private static bool Known(IPostscriptDictionary dict, in PostscriptValue key) =>
        dict.TryGet(key, out _);

    [PostscriptMethod("undef")]
    private static void Undefine(IPostscriptDictionary dict, in PostscriptValue key) =>
        dict.Undefine(key);

    [PostscriptMethod("where")]
    private static void Where(DictionaryStack dicts, OperandStack operands) =>
        dicts.PostscriptWhere(operands);

    [PostscriptMethod("currentdict")]
    private static PostscriptValue CurrentDictionary(DictionaryStack dict) =>
        dict.CurrentDictAsValue;

    [PostscriptMethod("userdict")]
    private static PostscriptValue UserDict(PostscriptEngine engine) =>
        engine.UserDict.AsPostscriptValue();

    [PostscriptMethod("globaldict")]
    private static PostscriptValue GlobalDict(PostscriptEngine engine) =>
        engine.GlobalDict.AsPostscriptValue();

    [PostscriptMethod("systemdict")]
    private static PostscriptValue SystemDict(PostscriptEngine engine) =>
        engine.SystemDict.AsPostscriptValue();

    [PostscriptMethod("countdictstack")]
    private static int CountDictStack(DictionaryStack dicts) => dicts.Count;

    [PostscriptMethod("dictstack")]
    private static PostscriptValue DictStack(DictionaryStack dicts, PostscriptArray target) =>
        dicts.WriteStackTo(target);

    [PostscriptMethod("cleardictstack")]
    private static void ClearDictionaryStack(DictionaryStack dicts) =>
        dicts.ResetToBottom3();

}