using Melville.INPC;

namespace Melville.Postscript.Interpreter.Values
{
    internal readonly partial struct ForcedLongString
    {
        [FromConstructor] public PostscriptValue Value { get; }

        public static implicit operator PostscriptValue(in ForcedLongString str) =>
            str.Value;
    }
}