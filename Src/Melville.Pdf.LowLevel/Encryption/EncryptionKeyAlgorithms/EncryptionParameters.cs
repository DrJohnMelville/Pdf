using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

public readonly struct EncryptionParameters
{
    public byte[] IdFirstElement {get;}
    public byte[] OwnerPasswordHash {get;}
    public byte[] UserPasswordHash {get;}
    public uint Permissions {get;}
    public int KeyLengthInBits {get;}
    public int KeyLengthInBytes => KeyLengthInBits / 8;

    public EncryptionParameters(byte[] idFirstElement, byte[] ownerPasswordHash, byte[] userPasswordHash, uint permissions, int keyLengthInBits)
    {
        IdFirstElement = idFirstElement;
        OwnerPasswordHash = ownerPasswordHash;
        UserPasswordHash = userPasswordHash;
        Permissions = permissions;
        KeyLengthInBits = keyLengthInBits;
    }

    public static async ValueTask<EncryptionParameters> Create(PdfDictionary trailer) =>
        (await trailer.GetOrNullAsync(KnownNames.ID).CA() is not PdfArray id ||
         await id[0].CA() is not PdfString firstId ||
         await trailer.GetOrNullAsync(KnownNames.Encrypt).CA() is not PdfDictionary dict ||
         await dict.GetOrNullAsync(KnownNames.O).CA() is not PdfString ownerHash ||
         await dict.GetOrNullAsync(KnownNames.U).CA() is not PdfString userHash ||
         await dict.GetOrNullAsync(KnownNames.P).CA() is not PdfNumber permissions ||
         await dict.GetOrNullAsync(KnownNames.Length).CA() is not PdfNumber length
        )? throw new PdfSecurityException("Required parameter missing for encryption"):
            new EncryptionParameters(
                firstId.Bytes, ownerHash.Bytes, userHash.Bytes, (uint)permissions.IntValue, 
                (int) length.IntValue);

    public override string ToString()
    {
        return $"IdFirstElt: {IdFirstElement.AsHex()}\r\n" +
               $"Owner: {OwnerPasswordHash.AsHex()}\r\n" +
               $"User: {UserPasswordHash.AsHex()}\r\n" +
               $"Permissions: {Permissions}\r\n" +
               $"KeyLengthInBits: {KeyLengthInBits}";
    }
}