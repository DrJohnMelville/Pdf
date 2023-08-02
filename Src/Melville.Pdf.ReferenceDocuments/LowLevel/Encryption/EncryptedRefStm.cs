
namespace Melville.Pdf.ReferenceDocuments.LowLevel.Encryption;

public class EncryptedRefStm: EncryptedFileWriter
{
    public EncryptedRefStm() : base("Encrypted file using a reference stream.",
        DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None))
    {
    }

    protected override Task WriteFileAsync(Stream target, PdfLowLevelDocument doc) => 
        doc.WriteToWithXrefStreamAsync(target, "User");

    protected override async ValueTask BuildDocumentAsync(PdfDocumentCreator builder)
    {
        await base.BuildDocumentAsync(builder);
        using (var b1 = builder.LowLevelCreator.ObjectStreamContext())
        {
            builder.LowLevelCreator.Add(PdfDirectObject.CreateString("String in Stream Context."u8));
        }
    }
}