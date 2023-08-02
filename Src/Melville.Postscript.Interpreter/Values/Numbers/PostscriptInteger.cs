using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Values.Numbers;


/// <summary>
/// PostscriptValueStrategy object representing long values
/// </summary>
[StaticSingleton()]
public partial class PostscriptInteger :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<long>,
    IPostscriptValueStrategy<int>,
    IPostscriptValueStrategy<double>,
    IPostscriptValueStrategy<float>,
    IPostscriptValueComparison
{
    string IPostscriptValueStrategy<string>.GetValue(in MementoUnion memento) => LongValue(memento).ToString();

    long IPostscriptValueStrategy<long>.GetValue(in MementoUnion memento) => (long)LongValue(memento);

    private static long LongValue(MementoUnion memento) => memento.Int64s[0];

    int IPostscriptValueStrategy<int>.GetValue(in MementoUnion memento) => (int)LongValue(memento);

    double IPostscriptValueStrategy<double>.GetValue(in MementoUnion memento) => (double)LongValue(memento);
    float IPostscriptValueStrategy<float>.GetValue(in MementoUnion memento) => (float)LongValue(memento);

    public int CompareTo(in MementoUnion memento, object otherStrategy, in MementoUnion otherMemento)
    {
        return (otherStrategy is IPostscriptValueStrategy<long> otherSpecificStrategy)?
            LongValue(memento).CompareTo(otherSpecificStrategy.GetValue(otherMemento)):
            throw new PostscriptNamedErrorException("Cannot convert other to long.","typecheck");
    }
}