using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption;

public abstract class EncryptedFileWriter : CreatePdfParser
{
    private ILowLevelDocumentEncryptor encryptor;
    protected EncryptedFileWriter(string prefix, string helpText, ILowLevelDocumentEncryptor encryptor) : 
        base(prefix, helpText)
    {
        this.encryptor = encryptor;
    }

    public override ValueTask WritePdfAsync(Stream target)
    { 
        var builder = new PdfDocumentCreator();
        builder.LowLevelCreator.AddEncryption(encryptor);
        var page = builder.Pages.CreatePage();
        page.AddStandardFont("F1", BuiltInFontName.TimesRoman, FontEncodingName.WinAnsiEncoding);
        page.AddToContentStream(new DictionaryBuilder()
            .WithFilter(FilterName.FlateDecode)
            .AsStream($"BT\n/F1 12 Tf\n100 700 Td\n({HelpText}) Tj\nET\n"));
        var doc = builder.CreateDocument();
        return new(WriteFile(target, doc));
    }

    protected virtual Task WriteFile(Stream target, PdfLowLevelDocument doc)
    {
        return doc.WriteToAsync(target, "User");
    }
}