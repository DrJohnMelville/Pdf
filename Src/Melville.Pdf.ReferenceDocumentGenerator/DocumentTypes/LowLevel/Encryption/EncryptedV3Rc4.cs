using System;
using System.IO;
using System.Threading.Tasks;
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
            await builder.CreateAttachedPageAsync($"BT\n/F1 24 Tf\n100 100 Td\n({HelpText}) Tj\nET\n");
            builder.FinalizePages();
            await WriteFile(target, builder);
        }

        protected virtual Task WriteFile(Stream target, PdfCreator builder)
        {
            return builder.Creator.CreateDocument().WriteToAsync(target, "User");
        }
    }
    public class EncryptedV3Rc4: EncryptedFileWriter
    {
        public EncryptedV3Rc4() : base(
            "-EncV3Rc4", "Document encrypted with Version 3 Rc4 standard security handler.",
            new DocumentEncryptorV3Rc4128("User", "Owner", PdfPermission.None))
        {
        }
    }
    public class EncryptedRefStm: EncryptedFileWriter
    {
        public EncryptedRefStm() : base(
            "-EncRefStm", "Encrypted file using a reference stream.",
            new DocumentEncryptorV3Rc4128("User", "Owner", PdfPermission.None))
        {
        }

        protected override Task WriteFile(Stream target, PdfCreator builder)
        {
            return builder.Creator.CreateDocument().WriteToWithXrefStreamAsync(target, "User");
        }
    }
    
}