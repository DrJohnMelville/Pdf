using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.ReferenceDocuments.Infrastructure;
using Melville.Pdf.ReferenceDocuments.LowLevel.Encryption;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption;

public class RoundTripEncryptedFiles
{

    [Fact]
    public void VerifyThatDefaultPasswordBytesRoundTripInPdfDocEncoding()
    {
        // this is an important property because it means that ComputeOwnerPasswordv2.UserKeyFromOwnerKey does not need
        // to trim the user password to its original length because the 32 bit password returned will round trip through
        // the string representation to a password that is equivilant to the original.
        Assert.True(BytePadder.PdfPasswordPaddingBytes.AsSpan().SequenceEqual(
            BytePadder.PdfPasswordPaddingBytes.PdfDocEncodedString().AsPdfDocBytes()));
    }
    
    private async Task TestEncryptedFileAsync(CreatePdfParser gen, int V, int R, int keyLengthInBits)
    {
        var target = await gen.AsMultiBufAsync();
        await VerifyUserPasswordWorksAsync(V, R, keyLengthInBits, gen.HelpText, target);
        await ParseTargetAsync(target, PasswordType.Owner, "Owner");
    }

    private async Task VerifyUserPasswordWorksAsync(int V, int R, int keyLengthInBits,
        string text, MultiBufferStream target)
    {
        var doc = await ParseTargetAsync(target, PasswordType.User, "User");
        var encrypt = await doc.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.EncryptTName);
        await VerifyNumberAsync(encrypt, KnownNames.VTName, V);
        await VerifyNumberAsync(encrypt, KnownNames.RTName, R);
        await VerifyNumberAsync(encrypt, KnownNames.LengthTName, keyLengthInBits);

        foreach (var indirectReference in doc.Objects.Values)
        {
            var obj = await indirectReference.LoadValueAsync();
            if (obj.TryGet(out PdfStream? ps) && ! ps.Keys.Contains(KnownNames.FilterTName) && ps.Keys.Count()==1)
            {
                await VerifyStreamContainsAsync(ps, text);
            }
        }
    }

    private async ValueTask VerifyStreamContainsAsync(PdfStream ps, string text)
    {
        await using var stream = await ps.StreamContentAsync(StreamFormat.PlainText);
        var streamSource = await new StreamReader(stream).ReadToEndAsync();
        Assert.Contains(text, streamSource);
    }

    private static ValueTask<PdfLoadedLowLevelDocument> ParseTargetAsync(
        MultiBufferStream target, PasswordType passwordType, string password) =>
        new PdfLowLevelReader(new ConstantPasswordSource(passwordType, password))
            .ReadFromAsync(target);

    private async ValueTask VerifyNumberAsync(PdfDictionary encrypt, PdfDirectObject PdfDirectObject, int expected)
    {
        var num = await encrypt.GetAsync<int>(PdfDirectObject);
        Assert.Equal(expected, num);
            
    }

    [Fact]
    public Task V1R3Rc4Async() => TestEncryptedFileAsync(new EncryptedV1Rc4(), 1,3, 40);
    [Fact]
    public Task V2R3Rc4Key128Async() => TestEncryptedFileAsync(new EncryptedR3Rc4(), 2,3,128);
    [Fact]
    public Task v1R2Rc440Async() => TestEncryptedFileAsync(new EncryptedR2Rc4(), 1,2,40);
    [Fact]
    public Task V2R3Rc4Key128Kb40Async() => TestEncryptedFileAsync(new EncryptedV3Rc4KeyBits40(), 2,3,40);
    [Fact]
    public Task EncRefStrAsync() => TestEncryptedFileAsync(new EncryptedRefStm(), 2, 3, 128);
    [Fact]
    public Task SimpleV4Rc4128Async() => TestEncryptedFileAsync(new Encryptedv4Rc4128(), 4, 4, 128);
    [Fact]
    public Task SimpleV4Aes128Async() => TestEncryptedFileAsync(new Encryptedv4Aes128(), 4, 4, 128);

    [Fact] public Task V4StreamsPlainAsync() => TestEncryptedFileAsync(new EncryptedV4StreamsPlain(), 4, 4, 128);
}