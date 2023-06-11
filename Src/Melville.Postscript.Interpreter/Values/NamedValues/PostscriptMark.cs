using System;
using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    [StaticSingleton]
    internal partial class PostscriptMark: IPostscriptValueStrategy<string>
    {
        public string GetValue(in Int128 memento) => "<Mark Object>";
    }
}