﻿using System.Text;

namespace Melville.Postscript.Interpreter.Values.Composites;

internal abstract class PostscriptComposite : IPostscriptComposite,
    IPostscriptValueStrategy<PostscriptComposite>,
    IPostscriptValueStrategy<string>
{
    public abstract bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result);

    public abstract void Put(in PostscriptValue indexOrKey, in PostscriptValue value);
    public abstract int Length { get; }

    public abstract PostscriptValue CopyFrom(PostscriptValue source, PostscriptValue target);

    public abstract ForAllCursor CreateForAllCursor();

    PostscriptComposite IPostscriptValueStrategy<PostscriptComposite>.GetValue(in MementoUnion memento) =>
        this;

    private bool inStringGen = false;
    string IPostscriptValueStrategy<string>.GetValue(in MementoUnion memento)
    {
        if (inStringGen) return "<Blocked Recursive String write.>";
        try
        {
            inStringGen = true;
            return StringRep();
        }
        finally
        {
            inStringGen = false;
        }
    }

    private string StringRep()
    {
        var builder = new StringBuilder();
        StringRep(builder);
        return builder.ToString();
    }

    protected abstract void StringRep(StringBuilder builder);
}