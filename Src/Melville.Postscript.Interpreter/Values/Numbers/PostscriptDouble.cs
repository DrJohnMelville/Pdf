using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton]
internal partial class PostscriptDouble :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<int>,
    IPostscriptValueStrategy<long>,
    IPostscriptValueStrategy<double>

{
    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento) => 
        DoubleFromMemento(memento).ToString();

    int IPostscriptValueStrategy<int>.GetValue(in Int128 memento) => 
        (int)OffsetForTruncationRounding(memento);
    long IPostscriptValueStrategy<long>.GetValue(in Int128 memento) =>
        (long)OffsetForTruncationRounding(memento);

    private double OffsetForTruncationRounding(Int128 memento)
    {
        var value = DoubleFromMemento(memento);
        return value + (value < 0?-0.5:0.5);
    }

    double IPostscriptValueStrategy<double>.GetValue(in Int128 memento) => 
        DoubleFromMemento(memento);

    private double DoubleFromMemento(Int128 memento) => 
        BitConverter.Int64BitsToDouble((long)memento);
}