using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class EncryptionAlgorithmsActuallyEncryptTest
{
    private async Task<string> WriteAsync(PdfLowLevelDocument doc)
    {
        var target = new MemoryStream();
        var writer = new LowLevelDocumentWriter(PipeWriter.Create(target), doc, "User");
        await writer.WriteAsync();
        return target.GetBuffer().AsSpan(0,(int)target.Length).ExtendedAsciiString();
    }

    [Test]
    public Task AesLength128Async() => 
        CreateAndTestDocumentAsync(DocumentEncryptorFactory.V4("User", "Owner", PdfPermission.None, EncryptorName.AESV2, 8));
    [Test]
    public Task Rc4Length128Async() => 
        CreateAndTestDocumentAsync(DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None));
    [Test]
    public Task Rc4Length40Async() => 
        CreateAndTestDocumentAsync(DocumentEncryptorFactory.V1R2Rc440("User", "Owner", PdfPermission.None));

    private async Task CreateAndTestDocumentAsync(ILowLevelDocumentEncryptor encryptionDeclaration)
    {
        var docBuilder = new LowLevelDocumentBuilder();
        docBuilder.AddToTrailerDictionary(KnownNames.ID, new PdfArray(
            PdfDirectObject.CreateString("12345678901234567890123456789012"u8),
            PdfDirectObject.CreateString("12345678901234567890123456789012"u8)));
        docBuilder.AddEncryption(encryptionDeclaration);
        var creator = docBuilder;

        docBuilder.Add(PdfDirectObject.CreateString("Encrypted String"u8));
        docBuilder.Add(new DictionaryBuilder().AsStream("This is an encrypted stream"));
        var doc = creator.CreateDocument();
        var str = await WriteAsync(doc);
        Assert.DoesNotContain("Encrypted String", str);
        Assert.DoesNotContain("encrypted stream", str);

        using var doc2 = await str.ParseWithPasswordAsync("User", PasswordType.User);
        var outstr = await doc2.Objects[(2, 0)].LoadValueAsync();
        Assert.Equal("Encrypted String", outstr.ToString());
        var stream = await doc2.Objects[(3, 0)].LoadValueAsync<PdfStream>();
        using var streamContentAsync = await stream.ReaderForContentAsync();
        Assert.Equal("This is an encrypted stream", await streamContentAsync.ReadAsStringAsync());

        using var sc2 = await stream.ReaderForContentAsync();
        Assert.Equal("This is an encrypted stream", await sc2.ReadAsStringAsync());

    }
}