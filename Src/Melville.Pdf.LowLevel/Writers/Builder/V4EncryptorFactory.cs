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
public class V4Encryptor: ComputeEncryptionDictionary
{
    private readonly PdfName defStream;
    private readonly PdfName defString;
    private readonly PdfDictionary cfs;

    public V4Encryptor(
        string userPassword, string ownerPassword, int keyLengthInBits, PdfPermission permissionsRestricted,
        PdfName defStream, PdfName defString, in V4CfDictionary cfs) : 
        base(userPassword, ownerPassword, 4,4, keyLengthInBits, permissionsRestricted, 
            new ComputeOwnerPasswordV3(), new ComputeUserPasswordV3(), new GlobalEncryptionKeyComputerV3())
    {
        this.defStream = defStream;
        this.defString = defString;
        this.cfs = cfs.Build();
    }

    protected override DictionaryBuilder DictionaryItems(PdfArray id)
    {
        var ret = base.DictionaryItems(id);
        ret.WithItem(KnownNames.CF, cfs);
        ret.WithItem(KnownNames.StrF, defString);
        ret.WithItem(KnownNames.StmF, defStream);
        return ret;
    }
}