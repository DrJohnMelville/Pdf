using System;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption
{
    public class EncryptedV3Rc4: CreatePdfParser
    {
        public EncryptedV3Rc4() : base("-EncV3Rc4", "Document encrypted with Version 3 Rc4 standard security handler.")
        {
        }

        protected override ValueTask WritePdf(Stream target)
        {
            throw new NotImplementedException();
        }
    }
}