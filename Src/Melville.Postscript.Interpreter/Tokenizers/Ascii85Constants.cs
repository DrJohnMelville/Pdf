namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal static class Ascii85Constants
    {
        public const byte FirstChar = (byte)'!';
        public const byte IncompleteGroupPadding = (byte)'u';
        public const byte FirstTerminatingChar = (byte)'~';
        public const byte SecondTerminatingChar = (byte)'>';
        public const byte FourZeroChar = (byte)'z';
    }
}