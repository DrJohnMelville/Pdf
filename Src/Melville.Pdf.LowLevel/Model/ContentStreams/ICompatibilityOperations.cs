namespace Melville.Pdf.LowLevel.Model.ContentStreams;

/// <summary>
/// Specifies the two operators that handle compatibility regions.
/// </summary>
public interface ICompatibilityOperations
{
    /// <summary>
    /// Implements PDF operator BX
    /// </summary>
    void BeginCompatibilitySection();
    /// <summary>
    /// Implements Pdf Operatpr EX
    /// </summary>
    void EndCompatibilitySection();
}