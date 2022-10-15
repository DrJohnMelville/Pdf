using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.ReferenceDocuments.LowLevel.Encryption;

public class EncryptedRefStm: EncryptedFileWriter
{
    public EncryptedRefStm() : base("Encrypted file using a reference stream.",
        DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None))
    {
    }

    protected override Task WriteFile(Stream target, PdfLowLevelDocument doc) => 
        doc.WriteToWithXrefStreamAsync(target, "User");

    protected override async ValueTask BuildDocument(PdfDocumentCreator builder)
    {
        await base.BuildDocument(builder);
        using (var b1 = builder.LowLevelCreator.ObjectStreamContext())
        {
            builder.LowLevelCreator.Add(PdfString.CreateAscii("String in Stream Context."));
        }
    }
}