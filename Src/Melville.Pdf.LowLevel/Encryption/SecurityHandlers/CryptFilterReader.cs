using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;
using Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

internal readonly partial struct CryptFilterReader
{
    [FromConstructor] private readonly IRootKeyComputer rootKeyComputer;
    [FromConstructor] private readonly PdfDictionary encryptionDictionary;
    [FromConstructor] private readonly PdfDictionary cfd;
    private readonly Dictionary<PdfName, ISecurityHandler> finalDictionary = new()        
    {
        { KnownNames.Identity, NullSecurityHandler.Instance }
    };

    
    public async ValueTask<Dictionary<PdfName, ISecurityHandler>> ParseCfDictionaryAsync()
    {
        await ReadAllFiltersAsync();

        await SetDefaultFiltersAsync().CA();
        return finalDictionary;
    }

    private async Task ReadAllFiltersAsync()
    {
        foreach (var entry in cfd)
        {
            finalDictionary.Add(entry.Key, await ReadSingleHandlerAsync(entry).CA());
        }
    }

    private async Task<ISecurityHandler> ReadSingleHandlerAsync(KeyValuePair<PdfName, ValueTask<PdfObject>> entry)
    {
        var cryptDictionary = (PdfDictionary)await entry.Value.CA();
        var cfm = await cryptDictionary.GetAsync<PdfName>(KnownNames.CFM).CA();
        var handler = CreateSubSecurityHandler(cfm, encryptionDictionary);
        return handler;
    }

    private async Task SetDefaultFiltersAsync()
    {
        await SetSingleDefaultFilterAsync(KnownNames.StmF).CA();
        await SetSingleDefaultFilterAsync(KnownNames.StrF).CA();
    }

    private async Task SetSingleDefaultFilterAsync(PdfName filterName) => 
        finalDictionary.Add(filterName, finalDictionary[await FindDefaultFilterAsync(filterName).CA()]);

    private ValueTask<PdfName> FindDefaultFilterAsync(PdfName name) => 
        encryptionDictionary.GetOrDefaultAsync(name, KnownNames.Identity);

    private ISecurityHandler CreateSubSecurityHandler(PdfName cfm, PdfObject dictionary) => 
        cfm.GetHashCode() switch
    {
        KnownNameKeys.V2 => new SecurityHandler(
            Rc4KeySpecializer.Instance, Rc4CipherFactory.Instance, rootKeyComputer, dictionary),
        KnownNameKeys.AESV2 => new SecurityHandler(
            AesKeySpecializer.AesInstance, AesCipherFactory.Instance, rootKeyComputer, dictionary),
         KnownNameKeys.AESV3 => new SecurityHandler(
             AesV6KeySpecializer.Instance, AesCipherFactory.Instance, rootKeyComputer, dictionary), 
        KnownNameKeys.None => NullSecurityHandler.Instance,
        _ => throw new PdfSecurityException("Unknown Security Handler Type: " + cfm)
    };

    public static async ValueTask<ISecurityHandler> CreateAsync(IRootKeyComputer rootKeyComputer, PdfDictionary encryptionDictionary)
    {
        var cfd = await encryptionDictionary.GetAsync<PdfDictionary>(KnownNames.CF).CA();
        var finalDictionary =
            await new CryptFilterReader(rootKeyComputer, encryptionDictionary, cfd)
                .ParseCfDictionaryAsync().CA();
        return new SecurityHandlerV4(rootKeyComputer ,finalDictionary);
    }
}