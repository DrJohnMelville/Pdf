using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Xunit;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class S7_6_5CryptFilters
{
    private async Task VerifyStringAndStreamEncodingAsync(bool hideStream, bool hideString,
        ILowLevelDocumentCreator creator, PdfDirectValue? cryptFilterTypeForStream = null)
    {
        creator.Add(PdfDirectValue.CreateString("plaintext string"u8));
        creator.Add(InsertedStream(creator, cryptFilterTypeForStream));
        var str = await creator.AsStringAsync();
        Assert.Equal(!hideString, str.Contains("plaintext string"));
        Assert.Equal(!hideStream, str.Contains("plaintext stream"));
        var doc = await str.ParseDocumentAsync();
        Assert.Equal(3, doc.Objects.Count);
        Assert.Equal("plaintext string", (await doc.Objects[(2, 0)].LoadValueAsync()).ToString());
        Assert.Equal("plaintext stream", await (
                await (
                    await doc.Objects[(3, 0)].LoadValueAsync().CA()).Get<PdfValueStream>().StreamContentAsync())
            .ReadAsStringAsync());
    }

    private PdfValueStream InsertedStream(
        IPdfObjectCreatorRegistry creator, PdfDirectValue? cryptFilterTypeForStream)
    {
        var builder = !cryptFilterTypeForStream.HasValue ?
            new ValueDictionaryBuilder():
            EncryptedStreamBuilder(cryptFilterTypeForStream.Value);

        return builder.AsStream("plaintext stream");
    }

    private static ValueDictionaryBuilder EncryptedStreamBuilder(PdfDirectValue cryptFilterTypeForStream)
    {
        return new ValueDictionaryBuilder()
            .WithFilter(FilterName.Crypt)
            .WithFilterParam(new ValueDictionaryBuilder()
                .WithItem(KnownNames.TypeTName, KnownNames.CryptFilterDecodeParmsTName)
                .WithItem(KnownNames.NameTName, cryptFilterTypeForStream).AsDictionary());
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task CanUseDefaultStringsInAsync(bool hideStream, bool hideString)
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            Encoder(hideStream), Encoder(hideString), Encoder(hideStream), new V4CfDictionary(KnownNames.V2TName, 16)));
        await VerifyStringAndStreamEncodingAsync(hideStream, hideString, creator);
    }

    [Fact]
    public Task UseIdentityCryptFilterToGEtOutOfStreamEncryptionAsync()
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            KnownNames.StdCFTName, KnownNames.StdCFTName, KnownNames.StmFTName, new V4CfDictionary(KnownNames.V2TName, 16)));
        return VerifyStringAndStreamEncodingAsync(false, true, creator, KnownNames.IdentityTName);
    }

    [Fact]
    public Task UseCryptFilterToOptIntoEncryptionAsync()
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            KnownNames.IdentityTName, KnownNames.StdCFTName, KnownNames.StmFTName, new V4CfDictionary(KnownNames.V2TName, 16)));
        return VerifyStringAndStreamEncodingAsync(true, true, creator, KnownNames.StdCFTName);
    }

    [Fact]
    public  Task StreamsWithoutCryptFilterGetDefaultEncryptionAsync()
    {
        var creator = LowLevelDocumentBuilderFactory.New();
        creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
            Encoder(true), Encoder(true),Encoder(true), new V4CfDictionary(KnownNames.NoneTName, 16)));
        return VerifyStringAndStreamEncodingAsync(false, false, creator);
            
    }
        
    private static PdfDirectValue Encoder(bool hideString) => 
        hideString?KnownNames.StdCFTName:KnownNames.IdentityTName;
}