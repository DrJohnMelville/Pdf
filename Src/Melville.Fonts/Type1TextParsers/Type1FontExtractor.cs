using System.Buffers;
using System.Net.Http.Headers;
using System.Numerics;
using Melville.Fonts.Type1TextParsers.EexecDecoding;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Fonts.Type1TextParsers;

internal ref struct Type1FontExtractor
{
    private readonly IPostscriptDictionary dictionary;
    private readonly IPostscriptDictionary privateDict;
    private int notDefIndex = 0;
    private int encodingPrefixBytes;

    public Type1FontExtractor(IPostscriptDictionary dict)
    {
        dictionary = dict;
        privateDict = dictionary.GetAs<IPostscriptDictionary>("/Private");
        encodingPrefixBytes = GetLenV();
    }

    public Type1GenericFont Extract()
    {
        var (names, charstrings) = ProcessCharString(
            dictionary.GetAs<IPostscriptDictionary>("/CharStrings"));
        return new Type1GenericFont(
            ReportedDictionary(),
            names, charstrings, notDefIndex,
            ComputeGlyphTransfor(), ReadSubrs(), ReadOtherSubrs());
    }

    private IPostscriptDictionary? ReportedDictionary()
    {
#if DEBUG
            return dictionary;
#else
        return null;
#endif
    }

    private Matrix3x2 ComputeGlyphTransfor() =>
        dictionary.TryGetAs("/FontMatrix", out IPostscriptArray? fontMatrix) &&
        fontMatrix is { Length: 6 } &&
        fontMatrix.TryGetAs(0, out float p0) &&
        fontMatrix.TryGetAs(1, out float p1) &&
        fontMatrix.TryGetAs(2, out float p2) &&
        fontMatrix.TryGetAs(3, out float p3) &&
        fontMatrix.TryGetAs(4, out float p4) &&
        fontMatrix.TryGetAs(5, out float p5)
            ? new Matrix3x2(p0, p1, p2, p3, p4, p5)
            : Matrix3x2.CreateScale(0.001f);

    private (string[] Names, Memory<byte>[] charstrings) ProcessCharString(
        IPostscriptDictionary charStrings)
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
            charDefinitions[i] = ReadCharString(items[1]);
        }

        ArrayPool<PostscriptValue>.Shared.Return(items);
        return (names, charDefinitions);
    }

    private Memory<byte> ReadCharString(PostscriptValue funcDecl)
    {
        var memory = funcDecl.Get<Memory<byte>>();
        if (memory.Length < encodingPrefixBytes) return memory;
        ushort charStringKey = 4330;
        DecodeType1Encoding.DecodeSegment(memory.Span, ref charStringKey);
        var decodedCharString = memory.Slice(encodingPrefixBytes);
        return decodedCharString;
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


    private Type1SubrExecutor ReadSubrs() => new(SubrsArray());

    private Memory<byte>[] SubrsArray()
    {
        if (!privateDict.TryGetAs("/Subrs", out IPostscriptArray? subrsSource))
            return [];
        var ret = new Memory<byte>[subrsSource.Length];

        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = ReadCharString(subrsSource.Get(i));
        }

        return ret;
    }


    private IPostscriptArray ReadOtherSubrs() => 
        privateDict.TryGetAs("/OtherSubrs", out IPostscriptArray? others) ? 
            others : PostscriptSingletosn.Array;
}