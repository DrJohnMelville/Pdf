
namespace Melville.Parsing.CountingReaders;

/// <summary>
/// Static class implementing extension methods on ShouldContinue.
/// </summary>
public static class ShouldContinueImpl
{
    /// <summary>
    /// This method enables a very specific pattern that is common with parsing from the PipeReader.
    ///
    /// the pattern is do{}while(source.ShouldContinue(Method(await source.ReadAsync)));
    ///
    /// Method returns a pair (bool, SequencePosition).  Method can use out parameters for "real"
    /// return values.
    ///
    /// This pattern repeately reads the stream until method successfully parses, then it advances
    /// the reader to the given sequence position.
    /// </summary>
    public static bool ShouldContinue(
        this IByteSource pipe, (bool Success, SequencePosition Position) result)
    {
        if (result.Success)
        {
            pipe.AdvanceTo(result.Position);
            return false;
        }

        pipe.MarkSequenceAsExamined();
        return true;
    }

}