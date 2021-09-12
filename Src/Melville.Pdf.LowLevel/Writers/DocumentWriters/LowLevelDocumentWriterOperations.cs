using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Document;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public static class LowLevelDocumentWriterOperations
    {
        public static Task WriteTo(this PdfLowLevelDocument doc, Stream s) => 
            new LowLevelDocumentWriter(PipeWriter.Create(s), doc).WriteAsync();
        public static Task WriteToWithXrefStream(this PdfLowLevelDocument doc, Stream s) => 
            new LowLevelDocumentWriter(PipeWriter.Create(s), doc).WriteWithReferenceStream();
    }
}