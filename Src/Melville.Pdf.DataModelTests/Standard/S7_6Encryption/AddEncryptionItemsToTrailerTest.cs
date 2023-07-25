using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class AddEncryptionItemsToTrailerTest
{
    private LowLevelDocumentBuilder docBuilder;
    private readonly PdfValueDictionary trailer;

    public AddEncryptionItemsToTrailerTest()
    {
        docBuilder = new LowLevelDocumentBuilder();
        docBuilder.AddToTrailerDictionary(KnownNames.IDTName, new PdfValueArray(
            PdfDirectValue.CreateString("12345678901234567890123456789012"u8),
            PdfDirectValue.CreateString("12345678901234567890123456789012"u8)));
        docBuilder.AddEncryption(DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None));
        trailer = docBuilder.CreateDocument().TrailerDictionary;
    }
        
    [Fact]
    public void EcryptionRequiresAnID()
    {
        Assert.True(trailer.ContainsKey(KnownNames.IDTName));
    }

    [Fact]
    public async Task EcryptionWithV3DictionaryAsync()
    {
        Assert.True(trailer.ContainsKey(KnownNames.EncryptTName));
        var dict = await trailer.GetAsync<PdfValueDictionary>(KnownNames.EncryptTName);
        Assert.Equal(2, (await dict.GetAsync<PdfNumber>(KnownNames.VTName)).IntValue);
        Assert.Equal(3, (await dict.GetAsync<PdfNumber>(KnownNames.RTName)).IntValue);
        Assert.Equal(128, (await dict.GetAsync<PdfNumber>(KnownNames.LengthTName)).IntValue);
        Assert.Equal(-1, (await dict.GetAsync<PdfNumber>(KnownNames.PTName)).IntValue);
        Assert.Equal(32, (await dict.GetAsync<PdfString>(KnownNames.UTName)).Bytes.Length);
        Assert.Equal(32, (await dict.GetAsync<PdfString>(KnownNames.OTName)).Bytes.Length);
        Assert.Equal(KnownNames.StandardTName, (await dict.GetAsync<PdfDirectValue>(KnownNames.FilterTName)));
    }
}