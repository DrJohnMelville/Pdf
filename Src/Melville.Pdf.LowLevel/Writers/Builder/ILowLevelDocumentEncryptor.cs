using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public interface ILowLevelDocumentEncryptor
{
    public PdfDictionary CreateEncryptionDictionary(PdfArray id);
    public string UserPassword { get; }
}

public class ComputeEncryptionDictionary : ILowLevelDocumentEncryptor
{
    private readonly string ownerPassword;
    public string UserPassword { get; set; }
    private readonly int permissions;
    private readonly IComputeOwnerPassword ownerPasswordComputer;
    private readonly IComputeUserPassword userPasswordComputer;
    private readonly IGlobalEncryptionKeyComputer keyComputer;
    private int v;
    private int r;
    private int keyLengthInBits;
    private int KeyLengthInBytes => keyLengthInBits / 8;

    public ComputeEncryptionDictionary(
        string userPassword,
        string ownerPassword,
        int v,
        int r,
        int keyLengthInBits,
        PdfPermission permissionsRestricted, 
        IComputeOwnerPassword ownerPasswordComputer, 
        IComputeUserPassword userPasswordComputer, 
        IGlobalEncryptionKeyComputer keyComputer)
    {
        this.UserPassword = userPassword;
        this.ownerPassword = ownerPassword;
        this.permissions = ~(int)permissionsRestricted;
        this.v = v;
        this.r = r;
        this.keyLengthInBits = keyLengthInBits;
        this.ownerPasswordComputer = ownerPasswordComputer;
        this.userPasswordComputer = userPasswordComputer;
        this.keyComputer = keyComputer;
    }
        
    public PdfDictionary CreateEncryptionDictionary(PdfArray id)
    {
        return DictionaryItems(id)
            .WithItem(KnownNames.Filter, KnownNames.Standard)
            .WithItem(KnownNames.V, 5) 
            .WithItem(KnownNames.R,6)
            .WithItem(KnownNames.Length, 256)
            .AsDictionary();
    }

    protected virtual DictionaryBuilder DictionaryItems(PdfArray id)
    {
        var dict = new DictionaryBuilder();
        var ownerHash = ownerPasswordComputer.ComputeOwnerKey(ownerPassword,UserPassword, KeyLengthInBytes);
        var ep = new EncryptionParameters(
            ((PdfString) id.RawItems[0]).Bytes, ownerHash, Array.Empty<byte>(),
            (uint)permissions, keyLengthInBits);
        dict.WithItem(KnownNames.Filter, KnownNames.Standard);
        dict.WithItem(KnownNames.V, new PdfInteger(v));
        if (keyLengthInBits > 0)
            dict.WithItem(KnownNames.Length, new PdfInteger(keyLengthInBits));
        dict.WithItem(KnownNames.P, new PdfInteger(permissions));
        dict.WithItem(KnownNames.R, new PdfInteger(r));
        dict.WithItem(KnownNames.U, new PdfString(UserHashForPassword(UserPassword, ep)));
        dict.WithItem(KnownNames.O, new PdfString(ownerHash));
        return dict;
    }

    public byte[] UserHashForPassword(in string userPassword, in EncryptionParameters parameters)
    {
        var key = keyComputer.ComputeKey(userPassword, parameters);
        var ret = userPasswordComputer.ComputeHash(key, parameters);
        return ret;
    }
}