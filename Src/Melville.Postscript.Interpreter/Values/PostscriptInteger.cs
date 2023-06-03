using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

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