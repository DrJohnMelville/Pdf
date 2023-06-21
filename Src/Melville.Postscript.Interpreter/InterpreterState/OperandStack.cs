using System.Collections.Generic;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Postscript.Interpreter.InterpreterState;

public sealed class OperandStack : PostscriptStack<PostscriptValue>
{
    public OperandStack() : base(0)
    {
    }

    /// <inheritdoc />
    protected override void MakeCopyable(ref PostscriptValue value) => 
        value = value.AsCopyableValue();

    internal void PushCount() => Push(Count);

    private static bool IsMark(PostscriptValue i) => i.IsMark;

    internal int CountToMark() => CountAbove(IsMark);

    internal void MarkedSpanToArray(bool asExecutable)
    {
        var array = PopTopToArray(CountToMark());
        Pop();
        var postscriptValue = PostscriptValueFactory.CreateArray(array);
        Push(asExecutable?postscriptValue.AsExecutable():postscriptValue);
    }

    internal void MarkedSpanToDictionary()
    {
        int count = CountToMark();
        var dict = PostscriptValueFactory.CreateDictionary(
            CollectionAsSpan()[^count..]);
        PopMultiple(count+1);
        Push(dict);
    }


    internal void CreatePackedArray() => 
        Push(
            PostscriptValueFactory.CreateArray(
                PopTopToArray(
                    Pop().Get<int>())));

    internal void ClearToMark() =>
        ClearThrough(IsMark);

    internal void PolymorphicCopy()
    {
        var topItem = Pop();
        if (topItem.TryGet(out IPostscriptComposite? dest))
            Push(dest.CopyFrom(Pop(), topItem));
        else
            CopyTop(topItem.Get<int>());
        
    }
}