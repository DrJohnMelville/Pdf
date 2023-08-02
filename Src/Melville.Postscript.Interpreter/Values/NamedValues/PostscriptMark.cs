using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    [StaticSingleton]
    internal partial class PostscriptMark: IPostscriptValueStrategy<string>
    {
        public string GetValue(in MementoUnion memento) => "<Mark Object>";
    }
}