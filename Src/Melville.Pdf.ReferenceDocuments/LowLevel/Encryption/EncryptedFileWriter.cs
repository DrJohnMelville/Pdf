using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocuments.Infrastructure;

namespace Melville.Pdf.ReferenceDocuments.LowLevel.Encryption;

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
        var builder = new PdfDocumentCreator();
        BuildEncryptedDocument.AddEncryption(builder.LowLevelCreator, encryptor);
        var page = builder.Pages.CreatePage();
        var font = page.AddStandardFont("F1", BuiltInFontName.TimesRoman, FontEncodingName.WinAnsiEncoding);
        await page.AddToContentStreamAsync(i =>
        {
            i.SetFont(font, 12);
            using var block = i.StartTextBlock();
            block.MovePositionBy(100, 700);
            block.ShowString(HelpText);
        });
        var doc = builder.CreateDocument();
        await WriteFile(target, doc);
    }

    protected virtual Task WriteFile(Stream target, PdfLowLevelDocument doc)
    {
        return doc.WriteToAsync(target, "User");
    }
}