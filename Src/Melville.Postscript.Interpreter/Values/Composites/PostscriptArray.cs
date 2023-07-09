using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Values;

internal partial class PostscriptArray : 
    PostscriptComposite,
    IPostscriptArray,
    IPostscriptValueStrategy<IExecutionSelector>
{
    public static readonly PostscriptArray Empty = new(Memory<PostscriptValue>.Empty);

    [FromConstructor] private readonly Memory<PostscriptValue> values;

    public PostscriptArray(int length): this(new PostscriptValue[length].AsMemory()) {}

    protected override void StringRep(StringBuilder sb)
    {
        sb.Append("[");
        foreach (var value in values.Span)
        {
            if (sb.Length > 1) sb.Append(" ");
            sb.Append(value.ToString());
        }
        sb.Append("]");
    }

    public Span<PostscriptValue> AsSpan() => values.Span;

    public override bool TryGet(
        in PostscriptValue indexOrKey, out PostscriptValue result) =>
        indexOrKey.TryGet(out int index) && index < values.Length
            ? values.Span[index].AsTrueValue(out result)
            : default(PostscriptValue).AsFalseValue(out result);

    public override void Put(
        in PostscriptValue indexOrKey, in PostscriptValue value) =>
        values.Span[indexOrKey.Get<int>()] = value;

    public override int Length => values.Length;

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

        return new PostscriptValue(this, PostscriptBuiltInOperations.PushArgument, default);
    }

    public void PushAllTo(PostscriptStack<PostscriptValue> stack)
    {
        foreach (var value in values.Span)
        {
            stack.Push(value);
        }
    }

    public override PostscriptValue CopyFrom(PostscriptValue source, PostscriptValue target)
    {
        if (!source.TryGet(out PostscriptArray? sourceArray))
            throw new PostscriptException("Cannot copy nonarray to an array");
        InsertAt(0, sourceArray);
        return InitialSubArray(sourceArray.Length, target.ExecutionStrategy);
    }

    public PostscriptValue InitialSubArray(int desiredLength, IExecutePostscript executable) =>
        new(
            Length == desiredLength? 
                this: new PostscriptArray(values[..desiredLength]),
            executable, default);

    IExecutionSelector IPostscriptValueStrategy<IExecutionSelector>.GetValue(in MementoUnion memento) => 
        ArrayExecutionSelector.Instance;

    public IEnumerator<PostscriptValue> GetEnumerator() => 
        new MemoryEnumerator(values);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
   
    public override ForAllCursor CreateForAllCursor() => new(values, 1);
}