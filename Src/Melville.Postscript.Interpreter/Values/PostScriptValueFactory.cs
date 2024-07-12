using System;
using System.Collections.Generic;
using Melville.Parsing.CountingReaders;
using Melville.Postscript.Interpreter.Values.Composites;
using Melville.Postscript.Interpreter.Values.Execution;
using Melville.Postscript.Interpreter.Values.Numbers;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// Create Postscript Values
/// </summary>
public static class PostscriptValueFactory
{
    /// <summary>
    /// Create a PostscriptValue representing a long.
    /// </summary>
    /// <param name="value">The long to encode</param>
    public static PostscriptValue Create(IByteSource value) =>
        new(value, PostscriptBuiltInOperations.PushArgument, default);
    /// <summary>
    /// Create a PostscriptValue representing a long.
    /// </summary>
    /// <param name="value">The long to encode</param>
    public static PostscriptValue Create(long value) =>
        new(PostscriptInteger.Instance, PostscriptBuiltInOperations.PushArgument, 
            MementoUnion.CreateFrom(value));

    /// <summary>
    /// Create a PostscriptValue representing a double.
    /// </summary>
    /// <param name="value">The double to encode</param>
    public static PostscriptValue Create(double value) =>
        new(PostscriptDouble.Instance, PostscriptBuiltInOperations.PushArgument,
            MementoUnion.CreateFrom(value));

    /// <summary>
    /// Create a PostscriptValue representing a boolean.
    /// </summary>
    /// <param name="value">The double to encode</param>
    public static PostscriptValue Create(bool value) =>
        new(PostscriptBoolean.Instance, PostscriptBuiltInOperations.PushArgument,
            MementoUnion.CreateFrom(value));

    /// <summary>
    /// Wrap an IExtenalFunction in a postscript value.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static PostscriptValue Create(IExternalFunction action) =>
        new(action, action, default);


    /// <summary>
    /// Create a PostScriptvalue with Null values.
    /// </summary>
    public static PostscriptValue CreateNull() => new(
        PostscriptNull.Instance, PostscriptBuiltInOperations.PushArgument, default);

    /// <summary>
    /// Create a PostscriptValue for the Mark object.
    /// </summary>
    public static PostscriptValue CreateMark() =>
        new(PostscriptMark.Instance, PostscriptBuiltInOperations.PushArgument, default);

    /// <summary>
    /// Create a string, name, or literal name
    /// </summary>
    /// <param name="data">Contents of the string</param>
    /// <param name="kind">kind of name to create</param>
    public static PostscriptValue CreateString(string data, StringKind kind) =>
        CreateString(data.AsSpan(), kind);

    /// <summary>
    /// Create a string, name, or literal name
    /// </summary>
    /// <param name="data">Contents of the string</param>
    /// <param name="kind">kind of name to create</param>
    public static PostscriptValue CreateString(
        ReadOnlySpan<char> data, StringKind kind)
    {
        Span<byte> buffer = stackalloc byte[data.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (byte)(data[i]);
        }
        return CreateString(buffer, kind);
    }


    /// <summary>
    /// Create a string, name, or literal name
    /// </summary>
    /// <param name="data">Contents of the string</param>
    /// <param name="kind">kind of name to create</param>
    public static PostscriptValue CreateString(in ReadOnlySpan<byte> data, StringKind kind)
    {
        var (strategy, memento) = data.PostscriptEncode(kind);

        return new PostscriptValue(strategy, kind.DefaultAction, memento);
    }

    /// <summary>
    /// Create a postscript long string which refers to the passed in Memory.
    /// </summary>
    /// <param name="data">Mempry&lt;byte&gt; that will be the backing store for the string</param>
    /// <param name="kind">Type of string to create</param>
    /// <returns>A postscript value referingt o the long  string.</returns>
    public static PostscriptValue CreateLongString(Memory<byte> data, StringKind kind) =>
        new(
            ReportAllocation(new PostscriptLongString(kind, data)), kind.DefaultAction, default);

    /// <summary>
    /// Create a new PdfArray
    /// </summary>
    /// <param name="size">The length of the array</param>
    public static PostscriptValue CreateSizedArray(int size)
    {
        var values = new PostscriptValue[size];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = CreateNull();
        }

        return CreateArray(values);
    }

    /// <summary>
    /// Create an array that takes and reuses an array of postscriptvalues
    /// </summary>
    /// <param name="values">The values to include in the postscript array.</param>
    /// <returns>A postscriptArray that owns the array passed in</returns>
    public static PostscriptValue CreateArray(params PostscriptValue[] values) =>
        new(WrapInPostScriptArray(values),
            PostscriptBuiltInOperations.PushArgument, default);

    private static PostscriptArray WrapInPostScriptArray(PostscriptValue[] values) =>
        values.Length < 1 ? PostscriptArray.Empty : ReportAllocation(new PostscriptArray(values));

    /// <summary>
    /// Create a dictionary 
    /// </summary>
    /// <param name="values">even numbered values are keys, odd numbered are values.</param>
    public static PostscriptValue CreateDictionary(params PostscriptValue[] values) =>
        CreateDictionary(values.AsSpan());

    /// <summary>
    /// Create a dictionary 
    /// </summary>
    /// <param name="values">even numbered values are keys, odd numbered are values.</param>
    public static PostscriptValue CreateDictionary(ReadOnlySpan<PostscriptValue> values) =>
        new(WrapInDictionary(values), PostscriptBuiltInOperations.PushArgument, default);

    private static IPostscriptValueStrategy<string> WrapInDictionary(
        ReadOnlySpan<PostscriptValue> values) =>
        values.Length switch
        {
            < 40 => ReportAllocation(new PostscriptShortDictionary(values)),
            _ => ReportAllocation(ConstructLongDictionary(values))
        };

    /// <summary>
    /// Force creation of a long dictionary.
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public static PostscriptValue CreateLongDictionary(params PostscriptValue[] parameters) =>
        new(ConstructLongDictionary(parameters), PostscriptBuiltInOperations.PushArgument, default);

    private static PostscriptLongDictionary ConstructLongDictionary(ReadOnlySpan<PostscriptValue> parameters) =>
        new(ArrayToDictionary(parameters));

    private static Dictionary<PostscriptValue, PostscriptValue> ArrayToDictionary(
        ReadOnlySpan<PostscriptValue> parameters)
    {
        var dict = new Dictionary<PostscriptValue, PostscriptValue>();
        for (var i = 1; i < parameters.Length; i += 2)
        {
            dict[parameters[i - 1]] = parameters[i];
        }
        return dict;
    }

    private static T ReportAllocation<T>(T item)
    {
        //right now this is a marker method.  Eventually to implement save and restore we have to
        // track all of the virtual memory allocations.  right now this method just holds onto 
        // all the places where we allocate memory.
        return item;
    }

    /// <summary>
    /// Creata an empty dictionary with a given size.
    /// </summary>
    /// <param name="size">The size of the new dictionary</param>
    /// <returns></returns>
    public static PostscriptValue CreateSizedDictionary(int size) => new(
        size <= 20
            ? new PostscriptShortDictionary(size)
            : new PostscriptLongDictionary(),
        PostscriptBuiltInOperations.PushArgument, default);
}