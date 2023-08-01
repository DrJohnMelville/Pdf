using System;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// Content stream operators that change the text state.
/// </summary>
public interface ITextObjectOperations
{
    /// <summary>
    /// Content stream operator Td
    /// </summary>
    void MovePositionBy(double x, double y);
    
    /// <summary>
    /// Content stream operator TD
    /// </summary>
    void MovePositionByWithLeading(double x, double y);

    /// <summary>
    /// Content Stream operator Tm
    /// </summary>
    void SetTextMatrix(double a, double b, double c, double d, double e, double f);

    /// <summary>
    /// Content stream operator T*
    /// </summary>
    void MoveToNextTextLine();

    /// <summary>
    /// Content stream operator Tj
    /// </summary>
    ValueTask ShowStringAsync(ReadOnlyMemory<byte> decodedString);

    /// <summary>
    /// Content stream operator '
    /// </summary>
    /// <param name="decodedString"></param>
    ValueTask MoveToNextLineAndShowStringAsync(ReadOnlyMemory<byte> decodedString);

    /// <summary>
    /// Content stream operator "
    /// </summary>
    /// <param name="wordSpace">the space before an 0x20 character</param>
    /// <param name="charSpace">the space between characters</param>
    /// <param name="decodedString"></param>
    ValueTask MoveToNextLineAndShowStringAsync(
        double wordSpace, double charSpace, ReadOnlyMemory<byte> decodedString);

    /// <summary>
    /// Context stream operator TJ
    ///
    /// </summary>
    ISpacedStringBuilder GetSpacedStringBuilder();
}

/// <summary>
/// This is a spaced string builder that the parser can use to pass multiple
/// doubles and strings to the renderer as part of a spaced string.  The parser may then
/// call SpacedStringComponentAsync methods as many times as needed until finally calling
/// DoneWritingAsync.  It is an error to call ISpacedStringBuilder methods after calling
/// DoneWritingAsync, to fail to call DoneWritingAsync, or to call any other rendering method
/// between the calls to GetSpacedStringBuilder and DoneWritingAsync.  Behavior is undefines
/// in all of these error cases
/// </summary>
public interface ISpacedStringBuilder
{
    /// <summary>
    /// Provide a double value to the spaced string builder.
    /// </summary>
    /// <param name="value">The double value.</param>
    public ValueTask SpacedStringComponentAsync(double value);

    /// <summary>
    /// Provide a string value to the spaced string builder.
    /// </summary>
    /// <param name="value">The string value to provide.</param>
    public ValueTask SpacedStringComponentAsync(Memory<byte> value);

    /// <summary>
    /// Indicate that no more data will be sent to the spaced string builder.
    /// </summary>
    public ValueTask DoneWritingAsync();
}

/// <summary>
/// Content stream operations to begin or end text blocks.
/// </summary>
public interface ITextBlockOperations
{
    /// <summary>
    /// Content stream operator BT
    /// </summary>
    void BeginTextObject();
    /// <summary>
    /// Content stream operator ET
    /// </summary>
    void EndTextObject();
}