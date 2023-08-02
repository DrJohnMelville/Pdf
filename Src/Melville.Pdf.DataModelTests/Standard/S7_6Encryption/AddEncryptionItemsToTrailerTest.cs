using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Postscript.Interpreter.Values;
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
            PdfDirectObject.CreateString("12345678901234567890123456789012"u8),
            PdfDirectObject.CreateString("12345678901234567890123456789012"u8)));
        docBuilder.AddEncryption(DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None));
        trailer = docBuilder.CreateDocument().TrailerDictionary;
    }
        
    [Fact]
    public void EcryptionRequiresAnID()
    {
        Assert.True(trailer.ContainsKey(KnownNames.ID));
    }

    [Fact]
    public async Task EcryptionWithV3DictionaryAsync()
    {
        Assert.True(trailer.ContainsKey(KnownNames.Encrypt));
        var dict = await trailer.GetAsync<PdfDictionary>(KnownNames.Encrypt);
        Assert.Equal(2, (await dict.GetAsync<int>(KnownNames.V)));
        Assert.Equal(3, (await dict.GetAsync<int>(KnownNames.R)));
        Assert.Equal(128, (await dict.GetAsync<int>(KnownNames.Length)));
        Assert.Equal(-1, (await dict.GetAsync<int>(KnownNames.P)));
        Assert.Equal(32, (await dict.GetAsync<StringSpanSource>(KnownNames.U)).GetSpan().Length);
        Assert.Equal(32, (await dict.GetAsync<StringSpanSource>(KnownNames.O)).GetSpan().Length);
        Assert.Equal(KnownNames.Standard, await dict[KnownNames.Filter]);
    }
}