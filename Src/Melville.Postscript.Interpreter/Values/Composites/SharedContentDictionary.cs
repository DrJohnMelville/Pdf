using System;
using System.Buffers;
using System.Text;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values.Composites;

internal sealed partial class SharedContentDictionary:
    IPostscriptDictionary,
    IPostscriptValueStrategy<string>
{
    [FromConstructor] private readonly IPostscriptDictionary innerDictionary;
    private readonly PostscriptDictionary localChanges = new PostscriptShortDictionary(6);


    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result) =>
        localChanges.TryGet(indexOrKey, out result) ||
        innerDictionary.TryGet(indexOrKey, out result);

    public void Put(in PostscriptValue indexOrKey, in PostscriptValue value) => 
        localChanges.Put(indexOrKey, value);

    public int Length => innerDictionary.Length + (localChanges?.Length ?? 0);

    public int MaxLength => innerDictionary.MaxLength + localChanges?.MaxLength ??3;

    public PostscriptValue CopyFrom(PostscriptValue source, PostscriptValue target) =>
        localChanges.CopyFrom(source, target);
        
    public ForAllCursor CreateForAllCursor()
    {
        var items = new PostscriptValue[Length * 2];

        var firstLen = CopyTo(
            innerDictionary.CreateForAllCursor(), items.AsSpan(), localChanges);
        var secondLen = CopyTo(
            localChanges.CreateForAllCursor(), items.AsSpan(firstLen), null);
       
        return new(items.AsMemory(0, firstLen+secondLen), 2);
    }

    private int CopyTo(ForAllCursor source, Span<PostscriptValue> destination,
        IPostscriptDictionary? exceptFor)
    {
        var localPos = 0;
        var targetPos = 0;
        while (source.TryGetItr(destination[targetPos..], ref localPos))
        {
            if (IsOverwrittenKey(exceptFor, destination[targetPos]))
                targetPos += 2;
        }
        return targetPos;
    }

    private static bool IsOverwrittenKey(IPostscriptDictionary? exceptFor, PostscriptValue key) => 
        exceptFor is null || !exceptFor.TryGet(key, out _);

    public void Undefine(PostscriptValue key) =>
        localChanges.Undefine(key);

    public string GetValue(in MementoUnion memento)
    {
        var ret = new StringBuilder();
        ret.AppendLine("<<");
        WriteTo(innerDictionary.CreateForAllCursor(), ret);
        WriteTo(localChanges.CreateForAllCursor(), ret);
        ret.Append(">>");
        return ret.ToString();
    }

    private void WriteTo(in ForAllCursor curson, StringBuilder target)
    {
        var pos = 0;
        var items = ArrayPool<PostscriptValue>.Shared.Rent(2);
        while (curson.TryGetItr(items.AsSpan(), ref pos))
        {
            target.AppendLine($"    {items[0]}: {items[1]}");
        }
        ArrayPool<PostscriptValue>.Shared.Return(items);
    }
}