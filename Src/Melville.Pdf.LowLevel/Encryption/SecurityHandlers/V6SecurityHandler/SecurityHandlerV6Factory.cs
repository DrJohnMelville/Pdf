using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers.V6SecurityHandler;

internal static class SecurityHandlerV6Factory
{
    public async static ValueTask<ISecurityHandler> CreateAsync(PdfValueDictionary dict) => 
        await CryptFilterReader.CreateAsync(await ReadV6KeysAsync(dict).CA(), dict).CA();

    private static async Task<RootKeyComputerV6> ReadV6KeysAsync(PdfValueDictionary dict) =>
        new(
            await ReadKeyAsync(dict, KnownNames.UTName, KnownNames.UETName).CA(),
            await ReadKeyAsync(dict, KnownNames.OTName, KnownNames.OETName).CA());

    private static async Task<V6EncryptionKey> ReadKeyAsync(
        PdfValueDictionary dict, PdfDirectValue hashName, PdfDirectValue encodedKeyName) => new (
        await dict.GetAsync<Memory<byte>>(hashName).CA(),
        await dict.GetAsync<Memory<byte>>(encodedKeyName).CA());
}