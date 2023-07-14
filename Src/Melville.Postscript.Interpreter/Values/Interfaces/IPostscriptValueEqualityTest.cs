using System;

namespace Melville.Postscript.Interpreter.Values;

internal interface IPostscriptValueEqualityTest
{
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