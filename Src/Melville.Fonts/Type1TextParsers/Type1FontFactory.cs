using System.Buffers;
using System.Numerics;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Fonts.Type1TextParsers;

public class Type1FontFactory : BuiltInFunction
{
    public Type1GenericFont? Font { get; private set; }

    public IReadOnlyList<IGenericFont> Result =>
        Font is null ? [] : Font;

    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
    {
        var top = engine.OperandStack.Pop();
        engine.OperandStack.Pop();
        engine.OperandStack.Push(top);
        Font = ExtractFont(top.Get<IPostscriptDictionary>());
    }

    private static Type1GenericFont ExtractFont(IPostscriptDictionary dict)
    {
        return new Type1FontExtractor(dict).Extract();
    }
}

internal ref struct Type1FontExtractor
{
    private readonly IPostscriptDictionary dictionary;
    private readonly IPostscriptDictionary privateDict;
    private int notDefIndex = 0;
    public Type1FontExtractor(IPostscriptDictionary dict)
    {
        dictionary = dict;
        privateDict = dictionary.GetAs<IPostscriptDictionary>("/Private");
    }

    public Type1GenericFont Extract()
    {
        var (names, charstrings) = ProcessCharString(
            dictionary.GetAs<IPostscriptDictionary>("/CharStrings"),
            GetLenV());
        return new Type1GenericFont(dictionary, names, charstrings, notDefIndex,
            ComputeGlyphTransfor());
    }

    private Matrix3x2 ComputeGlyphTransfor()
    {
        #warning actually reAD THE Array
        return Matrix3x2.CreateScale(0.001f);
    }

    private (string[] Names, Memory<byte>[] charstrings) ProcessCharString(
        IPostscriptDictionary charStrings, int encodingPrefixBytes)
    {
        var length = charStrings.Length;
        var names = new string[length];
        var charDefinitions = new Memory<byte>[length];

        int outerCount = 0;
        var items = ArrayPool<PostscriptValue>.Shared.Rent(2);
        var iterator = charStrings.CreateForAllCursor();
        for (int i = 0; iterator.TryGetItr(items, ref outerCount); i++)
        {
            names[i] = items[0].Get<string>();
            CheckForNotDef(i, names[i]);
            var memory = items[1].Get<Memory<byte>>();
            ushort charStringKey = 4330;
            DecodeType1Encoding.DecodeSegment(memory.Span, ref charStringKey);
            charDefinitions[i] = memory.Slice(encodingPrefixBytes);
        }
        ArrayPool<PostscriptValue>.Shared.Return(items);
        return (names, charDefinitions);
    }

    private void CheckForNotDef(int i, string name)
    {
        if (name.Equals(".notdef", StringComparison.Ordinal)) notDefIndex = i;
    }

    private int GetLenV() =>
        !privateDict.TryGet("/LenV", out PostscriptValue pv) &&
        pv.TryGet(out int result)
            ? result
            : 4;
}