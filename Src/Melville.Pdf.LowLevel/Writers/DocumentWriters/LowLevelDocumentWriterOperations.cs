using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Document;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters;

public static class LowLevelDocumentWriterOperations
{
    public static Task WriteToAsync(this PdfLowLevelDocument doc, Stream s, string userPassword = "") => 
        new LowLevelDocumentWriter(PipeWriter.Create(s), doc, userPassword).WriteAsync();
    public static Task WriteToWithXrefStreamAsync(
        this PdfLowLevelDocument doc, Stream s, string userPassword = "") => 
        new LowLevelDocumentWriter(PipeWriter.Create(s), doc, userPassword).WriteWithReferenceStream();
}