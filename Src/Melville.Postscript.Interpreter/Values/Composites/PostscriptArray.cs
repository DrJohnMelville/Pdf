using System;
using System.Collections.Generic;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Values;

internal partial class PostscriptArray : 
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<IPostscriptComposite>,
    IPostscriptValueStrategy<IPostscriptArray>,
    IPostscriptValueStrategy<PostscriptArray>,
    IPostscriptArray,
    IPostscriptValueStrategy<IExecutionSelector>
{
    public static readonly PostscriptArray Empty = new(Memory<PostscriptValue>.Empty);

    [FromConstructor] private readonly Memory<PostscriptValue> values;

    public PostscriptArray(int length): this(new PostscriptValue[length].AsMemory()) {}

    public string GetValue(in Int128 memento)
    {
        var sb = new StringBuilder();
        sb.Append("[");
        foreach (var value in values.Span)
        {
            if (sb.Length > 1) sb.Append(" ");
            sb.Append(value.ToString());
        }
        sb.Append("]");
        return sb.ToString();
    }

    IPostscriptComposite
        IPostscriptValueStrategy<IPostscriptComposite>.GetValue(in Int128 memento) => this;
    IPostscriptArray
        IPostscriptValueStrategy<IPostscriptArray>.GetValue(in Int128 memento) => this;
    PostscriptArray
        IPostscriptValueStrategy<PostscriptArray>.GetValue(in Int128 memento) => this;

    public bool TryGet(in PostscriptValue indexOrKey, out PostscriptValue result) =>
        indexOrKey.TryGet(out int index) && index < values.Length
            ? values.Span[index].AsTrueValue(out result)
            : default(PostscriptValue).AsFalseValue(out result);

    public void Put(in PostscriptValue indexOrKey, in PostscriptValue value) =>
        values.Span[indexOrKey.Get<int>()] = value;

    public int Length => values.Length;

    public IPostscriptValueStrategy<string> IntervalFrom(int beginningPosition, int length) =>
        new PostscriptArray(values.Slice(beginningPosition, length));

    public void InsertAt(int index, IPostscriptArray source)
    {
        if (source is not PostscriptArray other)
            throw new PostscriptException("Only arrays can be copied to arrays");

        other.values.Span.CopyTo(this.values.Span[index..]);
    }

    public PostscriptValue PushAllFrom(PostscriptStack<PostscriptValue> stack)
    {
        while (stack.Count > 0)
        {
            values.Span[stack.Count - 1] = stack.Pop();
        }

        return new PostscriptValue(this, PostscriptBuiltInOperations.PushArgument, 0);
    }

    public void PushAllTo(PostscriptStack<PostscriptValue> stack)
    {
        foreach (var value in values.Span)
        {
            stack.Push(value);
        }
    }

    public PostscriptValue CopyFrom(PostscriptValue source)
    {
        if (!source.TryGet(out PostscriptArray? sourceArray))
            throw new PostscriptException("Cannot copy nonarray to an array");
        InsertAt(0, sourceArray);
        return new PostscriptValue(
            Length == sourceArray.Length? 
                this: new PostscriptArray(values[..sourceArray.Length]),
            source.ExecutionStrategy, 0);
    }

    IExecutionSelector IPostscriptValueStrategy<IExecutionSelector>.GetValue(
        in Int128 memento) => 
        ArrayExecutionSelector.Instance;

    public IAsyncEnumerator<PostscriptValue> GetAsyncEnumerator() => 
        new AsyncMemoryEnumerator(values);
}