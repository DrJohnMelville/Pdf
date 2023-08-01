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
using Melville.Postscript.Interpreter.Values;
using Xunit;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

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
        Assert.Equal(2, (await dict.GetAsync<int>(KnownNames.VTName)));
        Assert.Equal(3, (await dict.GetAsync<int>(KnownNames.RTName)));
        Assert.Equal(128, (await dict.GetAsync<int>(KnownNames.LengthTName)));
        Assert.Equal(-1, (await dict.GetAsync<int>(KnownNames.PTName)));
        Assert.Equal(32, (await dict.GetAsync<StringSpanSource>(KnownNames.UTName)).GetSpan().Length);
        Assert.Equal(32, (await dict.GetAsync<StringSpanSource>(KnownNames.OTName)).GetSpan().Length);
        Assert.Equal(KnownNames.StandardTName, await dict[KnownNames.FilterTName]);
    }
}