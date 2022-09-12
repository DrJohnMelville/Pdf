using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class AddEncryptionItemsToTrailerTest
{
    private LowLevelDocumentBuilder docBuilder;
    private readonly PdfDictionary trailer;

    public AddEncryptionItemsToTrailerTest()
    {
        docBuilder = new LowLevelDocumentBuilder();
        docBuilder.AddToTrailerDictionary(KnownNames.ID, new PdfArray(
            PdfString.CreateAscii("12345678901234567890123456789012"),
            PdfString.CreateAscii("12345678901234567890123456789012")));
        docBuilder.AddEncryption(DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None));
        trailer = docBuilder.CreateTrailerDictionary();
    }
        
    [Fact]
    public void EcryptionRequiresAnID()
    {
        Assert.True(trailer.ContainsKey(KnownNames.ID));
    }

    [Fact]
    public async Task EcryptionWithV3Dictionary()
    {
        Assert.True(trailer.ContainsKey(KnownNames.Encrypt));
        var dict = await trailer.GetAsync<PdfDictionary>(KnownNames.Encrypt);
        Assert.Equal(2, (await dict.GetAsync<PdfNumber>(KnownNames.V)).IntValue);
        Assert.Equal(3, (await dict.GetAsync<PdfNumber>(KnownNames.R)).IntValue);
        Assert.Equal(128, (await dict.GetAsync<PdfNumber>(KnownNames.Length)).IntValue);
        Assert.Equal(-1, (await dict.GetAsync<PdfNumber>(KnownNames.P)).IntValue);
        Assert.Equal(32, (await dict.GetAsync<PdfString>(KnownNames.U)).Bytes.Length);
        Assert.Equal(32, (await dict.GetAsync<PdfString>(KnownNames.O)).Bytes.Length);
        Assert.Equal(KnownNames.Standard, (await dict.GetAsync<PdfName>(KnownNames.Filter)));
    }
}