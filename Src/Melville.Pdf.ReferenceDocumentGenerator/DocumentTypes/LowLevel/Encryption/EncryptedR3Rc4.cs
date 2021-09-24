using System;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption
{
    public class EncryptedR3Rc4: EncryptedFileWriter
    {
        public EncryptedR3Rc4() : base(
            "-EncV2R3Rc4", "Document encrypted with Revision 3 Rc4 standard security handler.",
            DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None))
        {
        }
    }    
    public class EncryptedR2Rc4: EncryptedFileWriter
    {
        public EncryptedR2Rc4() : base(
            "-EncV1R2Rc4", "Document encrypted with Version 1 Revision 2 Rc4 standard security handler.",
            DocumentEncryptorFactory.v1R2Rc440("User", "Owner", PdfPermission.None))
        {
        }
    }    
    public class EncryptedV3Rc4KeyBits40: EncryptedFileWriter
    {
        public EncryptedV3Rc4KeyBits40() : base(
            "-EncV2R3Rc4K40", "Document encrypted with Version 3 Rc4 standard security handler (40 bit key).",
            DocumentEncryptorFactory.V2R3Rc440("User", "Owner", PdfPermission.None))
        {
        }
    }
    public class EncryptedV1Rc4: EncryptedFileWriter
    {
        public EncryptedV1Rc4() : base(
            "-EncV1R3Rc4", "Document encrypted with Version 1(40 bit key).",
            DocumentEncryptorFactory.V1R3Rc440("User", "Owner", PdfPermission.None))
        {
        }
    }
}