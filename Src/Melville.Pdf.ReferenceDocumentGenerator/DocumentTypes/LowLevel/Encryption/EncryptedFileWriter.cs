using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption
{
    public abstract class EncryptedFileWriter : CreatePdfParser
    {
        private ILowLevelDocumentEncryptor encryptor;
        protected EncryptedFileWriter(string prefix, string helpText, ILowLevelDocumentEncryptor encryptor) : 
            base(prefix, helpText)
        {
            this.encryptor = encryptor;
        }

        public override async ValueTask WritePdfAsync(Stream target)
        { 
            var builder = new PdfCreator(1, 7);
            builder.Creator.AddEncryption(encryptor);
            builder.CreateAttachedPage(new DictionaryBuilder().AsStream($"BT\n/F1 12 Tf\n100 100 Td\n({HelpText}) Tj\nET\n"));
            builder.FinalizePages();
            await WriteFile(target, builder);
        }

        protected virtual Task WriteFile(Stream target, PdfCreator builder)
        {
            return builder.Creator.CreateDocument().WriteToAsync(target, "User");
        }
    }
}