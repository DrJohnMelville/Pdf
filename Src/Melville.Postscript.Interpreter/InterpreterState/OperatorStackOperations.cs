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

    public static void MarkedSpanToArray(this PostscriptStack<PostscriptValue> items)
    {
        var ops = items.CollectionAsSpan()[^ComputeCountToMark(items)..];
        var array = PostscriptValueFactory.CreateArray(ops.ToArray());
        items.ClearToMark();
        items.Push(array);
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
}