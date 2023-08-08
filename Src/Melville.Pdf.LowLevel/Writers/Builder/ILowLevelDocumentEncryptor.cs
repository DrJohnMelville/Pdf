using System;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// This class allows a LowLevelDocumentWriter to write an encrypted document.
/// </summary>
public interface ILowLevelDocumentEncryptor
{
    /// <summary>
    /// Create an encryption dictionary that describes the encryption in this document.
    /// </summary>
    /// <param name="id">The ID element from the document root.</param>
    /// <returns>The encryption dictionary</returns>
    public PdfDictionary CreateEncryptionDictionary(PdfArray id);
    /// <summary>
    /// The user password that will read a document encrypted with this encryptor.
    /// </summary>
    public string UserPassword { get; }
}

internal class ComputeEncryptionDictionary : ILowLevelDocumentEncryptor
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
            .WithItem(KnownNames.V, v) 
            .WithItem(KnownNames.R,r)
            .WithItem(KnownNames.Length, keyLengthInBits)
            .AsDictionary();
    }

    protected virtual DictionaryBuilder DictionaryItems(PdfArray id)
    {
        var dict = new DictionaryBuilder();
        var ownerHash = ownerPasswordComputer.ComputeOwnerKey(ownerPassword,UserPassword, KeyLengthInBytes);

        var ep = new EncryptionParameters(
             ExtractFirstStringMemory(id),ownerHash, Array.Empty<byte>(),
            (uint)permissions, keyLengthInBits);
        dict.WithItem(KnownNames.Filter, KnownNames.Standard);
        dict.WithItem(KnownNames.V, v);
        if (keyLengthInBits > 0)
            dict.WithItem(KnownNames.Length, keyLengthInBits);
        dict.WithItem(KnownNames.P, permissions);
        dict.WithItem(KnownNames.R, r);
        dict.WithItem(KnownNames.U, PdfDirectObject.CreateString(UserHashForPassword(UserPassword, ep)));
        dict.WithItem(KnownNames.O, PdfDirectObject.CreateString(ownerHash));
        return dict;
    }

    private Memory<byte> ExtractFirstStringMemory(PdfArray id) =>
        id.RawItems[0].TryGetEmbeddedDirectValue(out Memory<byte> ret)
            ? ret
            : throw new PdfParseException("Encryption dictionary must contain direct objects/");

    public byte[] UserHashForPassword(in string userPassword, in EncryptionParameters parameters)
    {
        var key = keyComputer.ComputeKey(userPassword, parameters);
        var ret = userPasswordComputer.ComputeHash(key, parameters);
        return ret;
    }
}