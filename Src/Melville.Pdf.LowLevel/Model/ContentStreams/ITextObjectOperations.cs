using System;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;

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
    /// </summary>
    ValueTask ShowSpacedStringAsync(in Span<ContentStreamValueUnion> values);

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