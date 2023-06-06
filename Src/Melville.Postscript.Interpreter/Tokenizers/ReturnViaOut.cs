namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal static class ReturnViaOut
    {
        public static bool FalseDefault<T>(out T result) =>
            WrapTry(default, false, out result!);

        public static bool AsTrueValue<T>(this T value, out T result) =>
            value.WrapTry(true, out result);

        public static bool AsFalseValue<T>(this T value, out T result) =>
            value.WrapTry(false, out result);
        public static bool WrapTry<T>(this T value, bool returnVal, out T result)
        {
            result = value;
            return returnVal;
        }
    }
}