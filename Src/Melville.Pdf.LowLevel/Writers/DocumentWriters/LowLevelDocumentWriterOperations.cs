using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Document;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

public static class LowLevelDocumentWriterOperations
{
    /// <summary>
    /// Convenience method to write a low level document to a stream using a classic Xref table;
    /// </summary>
    /// <param name="doc">The document to write</param>
    /// <param name="s">The stream to receive the document.</param>
    /// <param name="userPassword">The user password with which to encrypt the document</param>
    /// <returns>A task that signals completion of the operation</returns>
    public static Task WriteToAsync(this PdfLowLevelDocument doc, Stream s, string userPassword = "") => 
        new LowLevelDocumentWriter(PipeWriter.Create(s), doc, userPassword).WriteAsync();

    /// <summary>
    /// Convenience method to write a low level document to a stream using a classic Xref table;
    /// </summary>
    /// <param name="doc">The document to write</param>
    /// <param name="s">The stream to receive the document.</param>
    /// <param name="userPassword">The user password with which to encrypt the document</param>
    /// <returns>A task that signals completion of the operation</returns>
    public static Task WriteToWithXrefStreamAsync(
        this PdfLowLevelDocument doc, Stream s, string userPassword = "") => 
        new XrefStreamLowLevelDocumentWriter(PipeWriter.Create(s), doc, userPassword).WriteAsync();
}