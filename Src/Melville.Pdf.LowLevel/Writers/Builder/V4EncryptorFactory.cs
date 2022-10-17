using System.Collections.Generic;
using System.Runtime.InteropServices;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public readonly struct V4CfDictionary
{
    private readonly DictionaryBuilder items = new();

    public V4CfDictionary(PdfName cfm, int keyLengthInBytes, PdfName? authEvent = null)
    {
        AddDefinition(KnownNames.StdCF, cfm, keyLengthInBytes, authEvent);
    }

    public void AddDefinition(PdfName name, PdfName cfm, int lengthInBytes, PdfName? authEvent = null)
    {
        items.WithItem(name, CreateDefinition(cfm, lengthInBytes, authEvent??KnownNames.DocOpen));
    }

    private PdfObject CreateDefinition(PdfName cfm, int lengthInBytes, PdfName authEvent) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.AuthEvent, authEvent)
            .WithItem(KnownNames.CFM, cfm)
            .WithItem(KnownNames.Length, new PdfInteger(lengthInBytes))
            .AsDictionary();

    public PdfDictionary Build() => items.AsDictionary();
}

public class EncryptorWithCfsDictionary : ComputeEncryptionDictionary
{
    private readonly PdfName defStream;
    private readonly PdfName defString;
    private readonly PdfName defEmbeddedFile;
    private readonly PdfDictionary cfs;

    public EncryptorWithCfsDictionary(
        string userPassword, string ownerPassword, int v, int r, int keyLengthInBits, 
        PdfPermission permissionsRestricted, IComputeOwnerPassword ownerPasswordComputer, 
        IComputeUserPassword userPasswordComputer, IGlobalEncryptionKeyComputer keyComputer, 
        PdfName defStream, PdfName defString, PdfName defEmbeddedFile, V4CfDictionary cfs) : 
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
public class V4Encryptor: EncryptorWithCfsDictionary
{
    public V4Encryptor(
        string userPassword, string ownerPassword, int keyLengthInBits, PdfPermission permissionsRestricted,
        PdfName defStream, PdfName defString, PdfName defEmbeddedFile, in V4CfDictionary cfs) : 
        base(userPassword, ownerPassword, 4,4, keyLengthInBits, permissionsRestricted, 
            ComputeOwnerPasswordV3.Instance, new ComputeUserPasswordV3(), new GlobalEncryptionKeyComputerV3(),
            defStream, defString, defEmbeddedFile, cfs)
    {
    }
}