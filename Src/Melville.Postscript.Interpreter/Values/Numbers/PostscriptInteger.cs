using System;
using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values.Execution;

namespace Melville.Postscript.Interpreter.Values.Numbers;


[StaticSingleton()]
internal partial class PostscriptInteger :
    IPostscriptValueStrategy<string>,
    IPostscriptValueStrategy<long>,
    IPostscriptValueStrategy<int>,
    IPostscriptValueStrategy<double>,
    IPostscriptValueStrategy<float>
{
    string IPostscriptValueStrategy<string>.GetValue(in MementoUnion memento) => LongValue(memento).ToString();

    long IPostscriptValueStrategy<long>.GetValue(in MementoUnion memento) => (long)LongValue(memento);

    private static unsafe long LongValue(MementoUnion memento) => memento.Int64s[0];

    int IPostscriptValueStrategy<int>.GetValue(in MementoUnion memento) => (int)LongValue(memento);

    double IPostscriptValueStrategy<double>.GetValue(in MementoUnion memento) => (double)LongValue(memento);
    float IPostscriptValueStrategy<float>.GetValue(in MementoUnion memento) => (float)LongValue(memento);
}