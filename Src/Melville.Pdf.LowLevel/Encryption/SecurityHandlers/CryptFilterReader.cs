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

    
    public async ValueTask<Dictionary<PdfName, ISecurityHandler>> ParseCfDictionary()
    {
        await ReadAllFilters();

        await SetDefaultFilters().CA();
        return finalDictionary;
    }

    private async Task ReadAllFilters()
    {
        foreach (var entry in cfd)
        {
            finalDictionary.Add(entry.Key, await ReadSingleHandler(entry).CA());
        }
    }

    private async Task<ISecurityHandler> ReadSingleHandler(KeyValuePair<PdfName, ValueTask<PdfObject>> entry)
    {
        var cryptDictionary = (PdfDictionary)await entry.Value.CA();
        var cfm = await cryptDictionary.GetAsync<PdfName>(KnownNames.CFM).CA();
        var handler = CreateSubSecurityHandler(cfm, encryptionDictionary);
        return handler;
    }

    private async Task SetDefaultFilters()
    {
        await SetSingleDefaultFilter(KnownNames.StmF).CA();
        await SetSingleDefaultFilter(KnownNames.StrF).CA();
    }

    private async Task SetSingleDefaultFilter(PdfName filterName) => 
        finalDictionary.Add(filterName, finalDictionary[await FindDefaultFilter(filterName).CA()]);

    private ValueTask<PdfName> FindDefaultFilter(PdfName name) => 
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

    public static async ValueTask<ISecurityHandler> Create(IRootKeyComputer rootKeyComputer, PdfDictionary encryptionDictionary)
    {
        var cfd = await encryptionDictionary.GetAsync<PdfDictionary>(KnownNames.CF).CA();
        var finalDictionary =
            await new CryptFilterReader(rootKeyComputer, encryptionDictionary, cfd)
                .ParseCfDictionary().CA();
        return new SecurityHandlerV4(rootKeyComputer ,finalDictionary);
    }
}