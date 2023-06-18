using System;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.InterpreterState;

internal static class OperatorStackOperations
{
    public static void PushCount(this PostscriptStack<PostscriptValue> items) =>
        items.Push(items.Count);

    public static void ClearToMark(this PostscriptStack<PostscriptValue> items)
    {
        while (items.Count > 0)
        {
            if (items.Pop().IsMark) return;
        }
    }

    public static void CountToMark(this PostscriptStack<PostscriptValue> items) =>
        items.Push(ComputeCountToMark(items));

    private static int ComputeCountToMark(PostscriptStack<PostscriptValue> items)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[^(i + 1)].IsMark) return i;
        }

        return items.Count;
    }

    public static void MarkedSpanToArray(
        this PostscriptStack<PostscriptValue> items, bool asExecutable)
    {
        var ops = items.CollectionAsSpan()[^ComputeCountToMark(items)..];
        var array = PostscriptValueFactory.CreateArray(ops.ToArray());
        items.ClearToMark();
        items.Push(SetExecutableFlag(asExecutable, array));
    }

    private static PostscriptValue SetExecutableFlag(bool asExecutable, PostscriptValue array)
    {
        if (asExecutable) array = array.AsExecutable();
        return array;
    }

    public static void PolymorphicCopy(this PostscriptStack<PostscriptValue> items)
    {
        var topItem = items.Pop();
        if (topItem.TryGet(out IPostscriptComposite? destination))
        {
            items.Push(destination.CopyFrom(items.Pop()));
            return;
        }
        items.CopyTop(topItem.Get<int>());
    }

    public static void CreatePackedArray(this PostscriptStack<PostscriptValue> items)
    {
        int size = items.Pop().Get<int>();
        var buffer = new PostscriptValue[size];
        for (int i = size-1; i >= 0; i--)
        {
            buffer[i] = items.Pop();
        }
        items.Push(PostscriptValueFactory.CreateArray(buffer));
    }

    public static void Duplicate(this PostscriptStack<PostscriptValue> items)
    {
        TryPromoteString(ref items.CollectionAsSpan()[^1]);
        items.Push(items.Peek());
    }

    private static void TryPromoteString(ref PostscriptValue value)
    {
        value = value.TryMakeLongString();
    }

}