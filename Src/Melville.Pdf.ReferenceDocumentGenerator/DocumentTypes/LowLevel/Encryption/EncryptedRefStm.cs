using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption;

public class EncryptedRefStm: EncryptedFileWriter
{
    public EncryptedRefStm() : base(
        "-EncRefStm", "Encrypted file using a reference stream.",
        DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None))
    {
    }

    protected override Task WriteFile(Stream target, PdfLowLevelDocument doc) => 
        doc.WriteToWithXrefStreamAsync(target, "User");
}