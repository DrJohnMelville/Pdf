using System.Collections.Generic;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public readonly struct V4CfDictionary
{
    private readonly Dictionary<PdfName, PdfObject> items;

    public V4CfDictionary(PdfName cfm, int keyLengthInBytes, PdfName? authEvent = null)
    {
        items = new Dictionary<PdfName, PdfObject>();
        AddDefinition(KnownNames.StdCF, cfm, keyLengthInBytes, authEvent);
    }

    public void AddDefinition(PdfName name, PdfName cfm, int lengthInBytes, PdfName? authEvent = null)
    {
        items.Add(name, CreateDefinition(cfm, lengthInBytes, authEvent??KnownNames.DocOpen));
    }

    private PdfObject CreateDefinition(PdfName cfm, int lengthInBytes, PdfName authEvent) =>
        new DictionaryBuilder()
            .WithItem(KnownNames.AuthEvent, authEvent)
            .WithItem(KnownNames.CFM, cfm)
            .WithItem(KnownNames.Length, new PdfInteger(lengthInBytes))
            .AsDictionary();

    public PdfDictionary Build() => new(items);
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

    protected override Dictionary<PdfName, PdfObject> DictionaryItems(PdfArray id)
    {
        var ret = base.DictionaryItems(id);
        ret.Add(KnownNames.CF, cfs);
        ret.Add(KnownNames.StrF, defString);
        ret.Add(KnownNames.StmF, defStream);
        return ret;
    }
}