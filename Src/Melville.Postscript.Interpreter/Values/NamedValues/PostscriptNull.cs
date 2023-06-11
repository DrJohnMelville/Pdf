using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values;

[StaticSingleton]
internal partial class PostscriptNull : IPostscriptValueStrategy<string>
{
    public string GetValue(in Int128 memento) => "<Null>";
}