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
    IPostscriptValueStrategy<double>
{
    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento) => memento.ToString();

    long IPostscriptValueStrategy<long>.GetValue(in Int128 memento) => (long)memento;

    int IPostscriptValueStrategy<int>.GetValue(in Int128 memento) => (int)memento;

    double IPostscriptValueStrategy<double>.GetValue(in Int128 memento) => (double)memento;
}