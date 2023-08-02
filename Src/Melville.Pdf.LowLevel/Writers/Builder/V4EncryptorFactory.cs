using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Builder struct for the definition of a V4 encryption dictionaries.  This defines named sets of encryption settings.
/// </summary>
public readonly struct V4CfDictionary
{
    private readonly DictionaryBuilder items = new();

    /// <summary>
    /// Create a v4 dictionary
    /// </summary>
    /// <param name="cfm">Encryption algorithm for the default encryption.</param>
    /// <param name="keyLengthInBytes">Key length for the default encryption</param>
    /// <param name="authEvent">When the password would be checked.  (Melville.PDF does not honor this parameter.)</param>
    public V4CfDictionary(PdfDirectObject cfm, int keyLengthInBytes, PdfDirectObject authEvent = default) => 
        AddDefinition(KnownNames.StdCF, cfm, keyLengthInBytes, authEvent);

    /// <summary>
    /// Add a named encryption algorithm to the dictionary.
    /// </summary>
    /// <param name="name">Name of the encryption settings</param>
    /// <param name="cfm">Encryption algorithm  used</param>
    /// <param name="lengthInBytes">Length of the encryption key used.</param>
    /// <param name="authEvent">When the password would be checked.  (Melville.PDF does not honor this parameter.)</param>
    public void AddDefinition(
        PdfDirectObject name, PdfDirectObject cfm, int lengthInBytes, PdfDirectObject authEvent = default) => 
        items.WithItem(name, CreateDefinition(cfm, lengthInBytes, authEvent.IsNull?KnownNames.DocOpen:authEvent));

    private PdfDictionary CreateDefinition(PdfDirectObject cfm, int lengthInBytes, PdfDirectObject authEvent) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.AuthEvent, authEvent)
            .WithItem(KnownNames.CFM, cfm)
            .WithItem(KnownNames.Length, lengthInBytes)
            .AsDictionary();

    internal PdfDictionary Build() => items.AsDictionary();
}

internal class EncryptorWithCfsDictionary : ComputeEncryptionDictionary
{
    private readonly PdfDirectObject defStream;
    private readonly PdfDirectObject defString;
    private readonly PdfDirectObject defEmbeddedFile;
    private readonly PdfDictionary cfs;

    public EncryptorWithCfsDictionary(
        string userPassword, string ownerPassword, int v, int r, int keyLengthInBits, 
        PdfPermission permissionsRestricted, IComputeOwnerPassword ownerPasswordComputer, 
        IComputeUserPassword userPasswordComputer, IGlobalEncryptionKeyComputer keyComputer, 
        PdfDirectObject defStream, PdfDirectObject defString, PdfDirectObject defEmbeddedFile, V4CfDictionary cfs) : 
        base(userPassword, ownerPassword, v, r, keyLengthInBits, permissionsRestricted, 
            ownerPasswordComputer, userPasswordComputer, keyComputer)
    {
        this.defStream = defStream;
        this.defString = defString;
        this.defEmbeddedFile = defEmbeddedFile;
        this.cfs = cfs.Build();
    }
    
    protected override DictionaryBuilder DictionaryItems(PdfArray id)
    {
        var ret = base.DictionaryItems(id);
        ret.WithItem(KnownNames.CF, cfs);
        ret.WithItem(KnownNames.StrF, defString);
        ret.WithItem(KnownNames.StmF, defStream);
        ret.WithItem(KnownNames.EFF, defEmbeddedFile);
        return ret;
    }

}
internal class V4Encryptor: EncryptorWithCfsDictionary
{
    public V4Encryptor(
        string userPassword, string ownerPassword, int keyLengthInBits, PdfPermission permissionsRestricted,
        PdfDirectObject defStream, PdfDirectObject defString, PdfDirectObject defEmbeddedFile, in V4CfDictionary cfs) : 
        base(userPassword, ownerPassword, 4,4, keyLengthInBits, permissionsRestricted, 
            ComputeOwnerPasswordV3.Instance, new ComputeUserPasswordV3(), new GlobalEncryptionKeyComputerV3(),
            defStream, defString, defEmbeddedFile, cfs)
    {
    }
}