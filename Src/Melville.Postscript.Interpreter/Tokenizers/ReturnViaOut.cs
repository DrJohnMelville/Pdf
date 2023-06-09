namespace Melville.Postscript.Interpreter.Tokenizers;

/// <summary>
/// This is a helper class that helps with the pattern of o boolean return with an
/// out value.  It lets us wrap assigning the out variable into the return expression
/// </summary>
public static class ReturnViaOut
{
    /// <summary>
    /// Return false after setting Result to its default value.
    /// </summary>
    /// <typeparam name="T">The type of the parameter></typeparam>
    /// <param name="result">Out variable to be assigned</param>
    /// <returns>false</returns>
    public static bool FalseDefault<T>(out T result) =>
        WrapTry(default, false, out result!);

    /// <summary>
    /// Returns true after setting the second parameter to the first.
    /// </summary>
    /// <typeparam name="T">Type of the parameter passed</typeparam>
    /// <param name="value">The value to be assigned from</param>
    /// <param name="result">The out variable to be assigned to</param>
    /// <returns>True</returns>
    public static bool AsTrueValue<T>(this T value, out T result) =>
        value.WrapTry(true, out result);

    /// <summary>
    /// Returns false after setting the second parameter to the first.
    /// </summary>
    /// <typeparam name="T">Type of the parameter passed</typeparam>
    /// <param name="value">The value to be assigned from</param>
    /// <param name="result">The out variable to be assigned to</param>
    /// <returns>True</returns>
    public static bool AsFalseValue<T>(this T value, out T result) =>
        value.WrapTry(false, out result);

    private static bool WrapTry<T>(this T value, bool returnVal, out T result)
    {
        result = value;
        return returnVal;
    }
}