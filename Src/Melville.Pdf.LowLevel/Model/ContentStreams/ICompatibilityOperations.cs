namespace Melville.Pdf.LowLevel.Model.ContentStreams;

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