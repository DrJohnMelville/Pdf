using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption
{
    public class EncryptedV3Rc4: CreatePdfParser
    {
        public EncryptedV3Rc4() : base("-EncV3Rc4", "Document encrypted with Version 3 Rc4 standard security handler.")
        {
        }

        protected override async ValueTask WritePdfAsync(Stream target)
        {
            var builder = new PdfCreator(1, 7);
            builder.Creator.AddEncryption(new DocumentEncryptorV3Rc4128("User", "Owner", PdfPermission.None));
            await builder.CreateAttachedPageAsync("BT\n/F1 24 Tf\n100 100 Td\n(Uses V3 128 bit RC4 Encryption) Tj\nET\n");
            builder.FinalizePages();
            await builder.Creator.CreateDocument().WriteToAsync(target, "User");
        }
    }
}