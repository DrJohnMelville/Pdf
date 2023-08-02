using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers.V6SecurityHandler;

internal static class SecurityHandlerV6Factory
{
    public async static ValueTask<ISecurityHandler> CreateAsync(PdfDictionary dict) => 
        await CryptFilterReader.CreateAsync(await ReadV6KeysAsync(dict).CA(), dict).CA();

    private static async Task<RootKeyComputerV6> ReadV6KeysAsync(PdfDictionary dict) =>
        new(
            await ReadKeyAsync(dict, KnownNames.U, KnownNames.UE).CA(),
            await ReadKeyAsync(dict, KnownNames.O, KnownNames.OE).CA());

    private static async Task<V6EncryptionKey> ReadKeyAsync(
        PdfDictionary dict, PdfDirectObject hashName, PdfDirectObject encodedKeyName) => new (
        await dict.GetAsync<Memory<byte>>(hashName).CA(),
        await dict.GetAsync<Memory<byte>>(encodedKeyName).CA());
}