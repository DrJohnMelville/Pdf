using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Builder struct for the definition of a V4 encryption dictionaries.  This defines named sets of encryption settings.
/// </summary>
public readonly struct V4CfDictionary
{
    private readonly ValueDictionaryBuilder items = new();

    /// <summary>
    /// Create a v4 dictionary
    /// </summary>
    /// <param name="cfm">Encryption algorithm for the default encryption.</param>
    /// <param name="keyLengthInBytes">Key length for the default encryption</param>
    /// <param name="authEvent">When the password would be checked.  (Melville.PDF does not honor this parameter.)</param>
    public V4CfDictionary(PdfDirectValue cfm, int keyLengthInBytes, PdfDirectValue authEvent = default) => 
        AddDefinition(KnownNames.StdCFTName, cfm, keyLengthInBytes, authEvent);

    /// <summary>
    /// Add a named encryption algorithm to the dictionary.
    /// </summary>
    /// <param name="name">Name of the encryption settings</param>
    /// <param name="cfm">Encryption algorithm  used</param>
    /// <param name="lengthInBytes">Length of the encryption key used.</param>
    /// <param name="authEvent">When the password would be checked.  (Melville.PDF does not honor this parameter.)</param>
    public void AddDefinition(
        PdfDirectValue name, PdfDirectValue cfm, int lengthInBytes, PdfDirectValue authEvent = default) => 
        items.WithItem(name, CreateDefinition(cfm, lengthInBytes, authEvent.IsNull?KnownNames.DocOpenTName:authEvent));

    private PdfValueDictionary CreateDefinition(PdfDirectValue cfm, int lengthInBytes, PdfDirectValue authEvent) =>
        new ValueDictionaryBuilder()
            .WithItem(KnownNames.AuthEventTName, authEvent)
            .WithItem(KnownNames.CFMTName, cfm)
            .WithItem(KnownNames.LengthTName, lengthInBytes)
            .AsDictionary();

    internal PdfValueDictionary Build() => items.AsDictionary();
}

internal class EncryptorWithCfsDictionary : ComputeEncryptionDictionary
{
    private readonly PdfDirectValue defStream;
    private readonly PdfDirectValue defString;
    private readonly PdfDirectValue defEmbeddedFile;
    private readonly PdfValueDictionary cfs;

    public EncryptorWithCfsDictionary(
        string userPassword, string ownerPassword, int v, int r, int keyLengthInBits, 
        PdfPermission permissionsRestricted, IComputeOwnerPassword ownerPasswordComputer, 
        IComputeUserPassword userPasswordComputer, IGlobalEncryptionKeyComputer keyComputer, 
        PdfDirectValue defStream, PdfDirectValue defString, PdfDirectValue defEmbeddedFile, V4CfDictionary cfs) : 
        base(userPassword, ownerPassword, v, r, keyLengthInBits, permissionsRestricted, 
            ownerPasswordComputer, userPasswordComputer, keyComputer)
    {
        this.defStream = defStream;
        this.defString = defString;
        this.defEmbeddedFile = defEmbeddedFile;
        this.cfs = cfs.Build();
    }
    
    protected override ValueDictionaryBuilder DictionaryItems(PdfValueArray id)
    {
        var ret = base.DictionaryItems(id);
        ret.WithItem(KnownNames.CFTName, cfs);
        ret.WithItem(KnownNames.StrFTName, defString);
        ret.WithItem(KnownNames.StmFTName, defStream);
        ret.WithItem(KnownNames.EFFTName, defEmbeddedFile);
        return ret;
    }

}
internal class V4Encryptor: EncryptorWithCfsDictionary
{
    public V4Encryptor(
        string userPassword, string ownerPassword, int keyLengthInBits, PdfPermission permissionsRestricted,
        PdfDirectValue defStream, PdfDirectValue defString, PdfDirectValue defEmbeddedFile, in V4CfDictionary cfs) : 
        base(userPassword, ownerPassword, 4,4, keyLengthInBits, permissionsRestricted, 
            ComputeOwnerPasswordV3.Instance, new ComputeUserPasswordV3(), new GlobalEncryptionKeyComputerV3(),
            defStream, defString, defEmbeddedFile, cfs)
    {
    }
}