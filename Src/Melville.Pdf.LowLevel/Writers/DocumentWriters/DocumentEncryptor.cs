using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public interface IDocumentEncryptor
    {
        IObjectEncryptor CreateEncryptor(PdfIndirectObject parentObject);
    }

    public class NullDocumentEncryptor: IDocumentEncryptor
    {
        public static readonly NullDocumentEncryptor Instance = new();

        private NullDocumentEncryptor()
        {
        }

        public IObjectEncryptor CreateEncryptor(PdfIndirectObject parentObject) =>
            NullObjectEncryptor.Instance;
    }
}