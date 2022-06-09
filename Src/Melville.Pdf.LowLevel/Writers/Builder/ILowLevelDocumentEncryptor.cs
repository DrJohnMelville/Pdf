using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public interface ILowLevelDocumentEncryptor
{
    public PdfDictionary CreateEncryptionDictionary(PdfArray id);
    public byte[] UserPassword { get; }
}

public class ComputeEncryptionDictionary : ILowLevelDocumentEncryptor
{
    private readonly byte[] ownerPassword;
    public byte[] UserPassword { get; set; }
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
        this.UserPassword = userPassword.AsExtendedAsciiBytes();
        this.ownerPassword = ownerPassword.AsExtendedAsciiBytes();
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
        return new PdfDictionary(DictionaryItems(id));
    }

    protected virtual Dictionary<PdfName,PdfObject> DictionaryItems(PdfArray id)
    {
        var dict = new Dictionary<PdfName, PdfObject>();
        var ownerHash = ownerPasswordComputer.ComputeOwnerKey(ownerPassword,UserPassword, KeyLengthInBytes);
        var ep = new EncryptionParameters(
            ((PdfString) id.RawItems[0]).Bytes, ownerHash, Array.Empty<byte>(),
            (uint)permissions, keyLengthInBits);
        dict.Add(KnownNames.Filter, KnownNames.Standard);
        dict.Add(KnownNames.V, new PdfInteger(v));
        dict.Add(KnownNames.Length, new PdfInteger(keyLengthInBits));
        dict.Add(KnownNames.P, new PdfInteger(permissions));
        dict.Add(KnownNames.R, new PdfInteger(r));
        dict.Add(KnownNames.U, new PdfString(UserHashForPassword(UserPassword, ep)));
        dict.Add(KnownNames.O, new PdfString(
            ownerHash));
        return dict;
    }

    public byte[] UserHashForPassword(
        in ReadOnlySpan<byte> userPassword, in EncryptionParameters parameters)
    {
        var key = keyComputer.ComputeKey(userPassword, parameters);
        var ret = userPasswordComputer.ComputeHash(key, parameters);
        return ret;
    }
}