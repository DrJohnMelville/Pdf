using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Values;

/// <summary>
/// PostscriptValueStrategy representing a double value
/// </summary>
[StaticSingleton]
public partial class PostscriptDouble :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<int>,
    IPostscriptValueStrategy<long>,
    IPostscriptValueStrategy<double>,
    IPostscriptValueStrategy<float>,
    IPostscriptValueComparison
{
    string IPostscriptValueStrategy<string>.GetValue(in MementoUnion memento) => 
        DoubleFromMemento(memento).ToString();

    int IPostscriptValueStrategy<int>.GetValue(in MementoUnion memento) => 
        (int)OffsetForTruncationRounding(memento);
    long IPostscriptValueStrategy<long>.GetValue(in MementoUnion memento) =>
        (long)OffsetForTruncationRounding(memento);

    private double OffsetForTruncationRounding(in MementoUnion memento)
    {
        var value = DoubleFromMemento(memento);
        return value + (value < 0?-0.5:0.5);
    }

    double IPostscriptValueStrategy<double>.GetValue(in MementoUnion memento) => 
        DoubleFromMemento(memento);
    float IPostscriptValueStrategy<float>.GetValue(in MementoUnion memento) => 
        (float)DoubleFromMemento(memento);

    private double DoubleFromMemento(in MementoUnion memento) => 
        memento.Doubles[0];

    /// <inheritdoc />
    public int CompareTo(in MementoUnion memento, object otherStrategy, in MementoUnion otherMemento)
    {
        return (otherStrategy is IPostscriptValueStrategy<double> otherSpecificStrategy)?
            DoubleFromMemento(memento).CompareTo(otherSpecificStrategy.GetValue(otherMemento)):
            throw new PostscriptNamedErrorException("Cannot convert other to doubke.","typecheck");
    }
}