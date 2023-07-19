using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;
using Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

internal readonly partial struct CryptFilterReader
{
    [FromConstructor] private readonly IRootKeyComputer rootKeyComputer;
    [FromConstructor] private readonly PdfValueDictionary encryptionDictionary;
    [FromConstructor] private readonly PdfValueDictionary cfd;
    private readonly Dictionary<PdfDirectValue, ISecurityHandler> finalDictionary = new()        
    {
        { KnownNames.IdentityTName, NullSecurityHandler.Instance }
    };

    
    public async ValueTask<Dictionary<PdfDirectValue, ISecurityHandler>> ParseCfDictionaryAsync()
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

    private async Task<ISecurityHandler> ReadSingleHandlerAsync(KeyValuePair<PdfDirectValue, ValueTask<PdfDirectValue>> entry)
    {
        var cryptDictionary = (await entry.Value).Get<PdfValueDictionary>();
        var cfm = await cryptDictionary[KnownNames.CFMTName].CA();
        var handler = CreateSubSecurityHandler(cfm, encryptionDictionary);
        return handler;
    }

    private async Task SetDefaultFiltersAsync()
    {
        await SetSingleDefaultFilterAsync((PdfDirectValue)KnownNames.IdentityTName).CA();
        await SetSingleDefaultFilterAsync((PdfDirectValue)KnownNames.IdentityTName).CA();
    }

    private async Task SetSingleDefaultFilterAsync(PdfDirectValue filterName) => 
        finalDictionary.Add(filterName, finalDictionary[await FindDefaultFilterAsync(filterName).CA()]);

    private ValueTask<PdfDirectValue> FindDefaultFilterAsync(PdfDirectValue name) => 
        encryptionDictionary.GetOrDefaultAsync(name, (PdfDirectValue)KnownNames.IdentityTName);
    
    private ISecurityHandler CreateSubSecurityHandler(PdfDirectValue cfm, PdfValueDictionary dictionary) => 
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

    public static async ValueTask<ISecurityHandler> CreateAsync(IRootKeyComputer rootKeyComputer, PdfValueDictionary encryptionDictionary)
    {
        var cfd = await encryptionDictionary.GetAsync<PdfValueDictionary>(KnownNames.CFTName).CA();
        var finalDictionary =
            await new CryptFilterReader(rootKeyComputer, encryptionDictionary, cfd)
                .ParseCfDictionaryAsync().CA();
        return new SecurityHandlerV4(rootKeyComputer,finalDictionary);
    }
}