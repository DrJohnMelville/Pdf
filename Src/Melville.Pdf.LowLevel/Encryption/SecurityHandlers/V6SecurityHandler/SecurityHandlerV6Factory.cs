using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers.V6SecurityHandler;

public static class SecurityHandlerV6Factory
{
    public async static ValueTask<ISecurityHandler> Create(PdfDictionary dict) => 
        await CryptFilterReader.Create(await ReadV6Keys(dict).CA(), dict).CA();

    private static async Task<RootKeyComputerV6> ReadV6Keys(PdfDictionary dict) =>
        new(
            await ReadKey(dict, KnownNames.U, KnownNames.UE).CA(),
            await ReadKey(dict, KnownNames.O, KnownNames.OE).CA());

    private static async Task<V6EncryptionKey> ReadKey(
        PdfDictionary dict, PdfName hashName, PdfName encodedKeyName) => new (
        (await dict.GetAsync<PdfString>(hashName).CA()).Bytes,
        (await dict.GetAsync<PdfString>(encodedKeyName).CA()).Bytes);
}