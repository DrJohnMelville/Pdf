using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

internal readonly partial struct EncryptionParameters
{
    [FromConstructor]public byte[] IdFirstElement {get;}
    [FromConstructor]public byte[] OwnerPasswordHash {get;}
    [FromConstructor]public byte[] UserPasswordHash {get;}
    [FromConstructor]public uint Permissions {get;}
    [FromConstructor]public int KeyLengthInBits {get;}
    public int KeyLengthInBytes => KeyLengthInBits / 8;
    
    public static async ValueTask<EncryptionParameters> Create(PdfDictionary trailer) =>
        (await trailer.GetOrNullAsync(KnownNames.ID).CA() is not PdfArray id ||
         await id[0].CA() is not PdfString firstId ||
         await trailer.GetOrNullAsync(KnownNames.Encrypt).CA() is not PdfDictionary dict ||
         await dict.GetOrNullAsync(KnownNames.O).CA() is not PdfString ownerHash ||
         await dict.GetOrNullAsync(KnownNames.U).CA() is not PdfString userHash ||
         await dict.GetOrNullAsync(KnownNames.P).CA() is not PdfNumber permissions
        )? throw new PdfSecurityException("Required parameter missing for encryption"):
            new EncryptionParameters(
                firstId.Bytes, ownerHash.Bytes, userHash.Bytes, (uint)permissions.IntValue, 
                (int)await dict.GetOrDefaultAsync(KnownNames.Length, 40).CA());

    public override string ToString() =>
        $"""
        IdFirstElt: {IdFirstElement.AsHex()}
        Owner: {OwnerPasswordHash.AsHex()}
        User: {UserPasswordHash.AsHex()}
        Permissions: {Permissions}
        KeyLengthInBits: {KeyLengthInBits}
        """;
}