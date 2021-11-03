using System;

namespace Melville.Pdf.LowLevel.Model.ContentStreams;

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

    void ShowString(ReadOnlySpan<byte> decodedString);
}

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