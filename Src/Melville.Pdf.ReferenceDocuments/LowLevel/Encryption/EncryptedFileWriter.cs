namespace Melville.Pdf.ReferenceDocuments.LowLevel.Encryption;

public abstract class EncryptedFileWriter : CreatePdfParser
{
    private ILowLevelDocumentEncryptor encryptor;
    public override string Password => "User";

    protected EncryptedFileWriter(string helpText, ILowLevelDocumentEncryptor encryptor) : 
        base(helpText)
    {
        this.encryptor = encryptor;
    }

    public override async ValueTask WritePdfAsync(Stream target)
    { 
        var builder = new PdfDocumentCreator();
        await BuildDocumentAsync(builder);
        var doc = builder.CreateDocument();
        await WriteFileAsync(target, doc);
    }

    protected virtual async ValueTask BuildDocumentAsync(PdfDocumentCreator builder)
    {
        BuildEncryptedDocument.AddEncryption(builder.LowLevelCreator, encryptor);
        var page = builder.Pages.CreatePage();
        var font = page.AddStandardFont("F1", BuiltInFontName.TimesRoman, FontEncodingName.WinAnsiEncoding);
        await page.AddToContentStreamAsync(i =>
        {
            i.SetFontAsync(font, 12);
            using var block = i.StartTextBlock();
            block.MovePositionBy(100, 700);
            block.ShowStringAsync(HelpText);
        });
    }

    protected virtual Task WriteFileAsync(Stream target, PdfLowLevelDocument doc)
    {
        return doc.WriteToAsync(target, "User");
    }
}