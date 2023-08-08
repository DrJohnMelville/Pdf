using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// Test if this postscript value strategy with the given memento is equivilent to another strategy and memento.
/// </summary>
public interface IPostscriptValueEqualityTest
{
    /// <summary>
    /// Test for equality between strategy / memento pairs
    /// </summary>
    /// <param name="memento">The memento associated with this strategy.</param>
    /// <param name="otherStrategy">The other strategy object</param>
    /// <param name="otherMemento">The other memento </param>
    /// <returns>true if (this, memento) has the same value as (otherStrategy, otherMemento) false otherwise.</returns>
    public bool Equals(in MementoUnion memento, object otherStrategy, in MementoUnion otherMemento);
}

/// <summary>
/// Allows Postscript values to define an ordering.  This is used in PdfTrees
/// </summary>
public interface IPostscriptValueComparison
{
    /// <summary>
    /// Compare this item to another item
    /// </summary>
    /// <param name="memento">Memento for this item</param>
    /// <param name="otherStrategy">Strategy for the other item</param>
    /// <param name="otherMemento">Memento for the othe item</param>
    /// <returns></returns>
    /// <exception cref="PostscriptNamedErrorException">If the values are not comparable</exception>
    int CompareTo(in MementoUnion memento, object otherStrategy, in MementoUnion otherMemento);
}