using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    [StaticSingleton]
    internal partial class PostscriptDouble :
        IPostscriptValueStrategy<string>,
        IPostscriptValueStrategy<long>,
        IPostscriptValueStrategy<double>
    {
        string IPostscriptValueStrategy<string>.GetValue(in Int128 memento) => 
            DoubleFromMemento(memento).ToString();

        long IPostscriptValueStrategy<long>.GetValue(in Int128 memento) => 
            RoundToLong(DoubleFromMemento(memento));

        private long RoundToLong(double value) => (long)(value + (value < 0?-0.5:0.5));

        double IPostscriptValueStrategy<double>.GetValue(in Int128 memento) => 
            DoubleFromMemento(memento);

        private double DoubleFromMemento(Int128 memento) => 
            BitConverter.Int64BitsToDouble((long)memento);
    }
}