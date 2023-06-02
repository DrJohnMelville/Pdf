using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton()]
internal partial class PostscriptInteger : 
    IPostscriptValueStrategy<string>, IPostscriptValueStrategy<long>, 
    IPostscriptValueStrategy<double>
{
    string IPostscriptValueStrategy<string>.GetValue(in Int128 memento) => memento.ToString();

    long IPostscriptValueStrategy<long>.GetValue(in Int128 memento) => (long)memento;

    double IPostscriptValueStrategy<double>.GetValue(in Int128 memento) => (double)memento;
}