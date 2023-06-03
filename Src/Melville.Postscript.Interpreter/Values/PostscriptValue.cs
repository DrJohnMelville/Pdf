using System;
using System.Diagnostics.CodeAnalysis;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

// public interface IPostscriptStringStrategy
// {
//     int StringLength(Int128 memento);
//     void FillString(Int128 memento, Span<byte> target);
// }

/// <summary>
/// This structure represents a single postscript value, most of which can be contained
/// internally, but some of which cannot.  The managedValue object allows for extensions like
/// dictionaries, files, and arrays.
/// </summary>
public readonly partial struct PostscriptValue: IEquatable<PostscriptValue>
{
    /// <summary>
    /// The strategy that can retrieve the value for this item.
    /// </summary>
    [FromConstructor]private readonly IPostscriptValueStrategy<string> valueStrategy;
    /// <summary>
    /// A 128 bit space that allows most values to be stored without a heap allocation.
    /// </summary>
    [FromConstructor]private readonly Int128 memento;

    /// <summary>
    /// True if this is a Postscript null object, false otherwise.
    /// </summary>
    public bool IsNull => valueStrategy is PostscriptNull;

    /// <summary>
    /// True if this is a Postscript mark object, false otherwise.
    /// </summary>
    public bool IsMark => valueStrategy is PostscriptMark;

    /// <summary>
    /// Gets a pdf value of a given type.
    /// </summary>
    /// <typeparam name="T">The type of result expected from this value</typeparam>
    /// <returns>The exoected value</returns>
    /// <exception cref="InvalidPostscriptTypeException">
    /// If the current value cannot be converted to the requested type.</exception>
    public T Get<T>() =>
        (valueStrategy as IPostscriptValueStrategy<T> ?? TypeError<T>()).GetValue(memento);

    private IPostscriptValueStrategy<T> TypeError<T>() =>
        throw new InvalidPostscriptTypeException(
            $"{valueStrategy.GetType()} does not implement IPostScriptValueStrategy<{typeof(T)}>");

    /// <summary>
    /// Try to extract a value of a given type from a PostscriptValue
    /// </summary>
    /// <typeparam name="T">The desired type</typeparam>
    /// <param name="value">Receives the converted value, if available</param>
    /// <returns>True if the value can be converted to the given type, false otherwise</returns>
    public bool TryGet<T>([NotNullWhen(true)]out T? value)
    {
        value = default;
        if (valueStrategy is IPostscriptValueStrategy<T> typedStrategy)
        {
            value = typedStrategy.GetValue(memento)!;
            return true;
        }
        return false;
    }

    public bool Equals(PostscriptValue other)
    {
        if (ShallowEqual(other) || DeepEqual(other)) return true;
        return false;
    }


    private bool ShallowEqual(PostscriptValue other) =>
        valueStrategy == other.valueStrategy &&
         memento == other.memento;

    private bool DeepEqual(PostscriptValue other)
    {
        return valueStrategy is IPostscriptValueComparison psc &&
               psc.Equals(memento, other.valueStrategy, other.memento);
    }

    public override int GetHashCode() => HashCode.Combine(valueStrategy, memento);

    /// <inheritdoc />
    public override string ToString() => valueStrategy.GetValue(memento);
}