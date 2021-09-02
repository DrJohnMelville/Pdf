using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Encryption
{
    public readonly struct EncryptionParameters
    {
        public byte[] IdFirstElement {get;}
        public byte[] OwnerPasswordHash {get;}
        public byte[] UserPasswordHash {get;}
        public uint Permissions {get;}
        public int KeyLengthInBits {get;}

        public EncryptionParameters(byte[] idFirstElement, byte[] ownerPasswordHash, byte[] userPasswordHash, uint permissions, int keyLengthInBits)
        {
            IdFirstElement = idFirstElement;
            OwnerPasswordHash = ownerPasswordHash;
            UserPasswordHash = userPasswordHash;
            Permissions = permissions;
            KeyLengthInBits = keyLengthInBits;
        }

        public static async ValueTask<EncryptionParameters> Create(PdfDictionary trailer) =>
            (await trailer.GetOrNullAsync(KnownNames.ID) is not PdfArray id ||
             await id[0] is not PdfString firstId ||
             await trailer.GetOrNullAsync(KnownNames.Encrypt) is not PdfDictionary dict ||
             await dict.GetOrNullAsync(KnownNames.O) is not PdfString ownerHash ||
             await dict.GetOrNullAsync(KnownNames.U) is not PdfString userHash ||
             await dict.GetOrNullAsync(KnownNames.P) is not PdfNumber permissions ||
             await dict.GetOrNullAsync(KnownNames.Length) is not PdfNumber length
            )? throw new PdfSecurityException("Required parameter missing for encryption"):
                new EncryptionParameters(
                    firstId.Bytes, ownerHash.Bytes, userHash.Bytes, (uint)permissions.IntValue, 
                    (int) length.IntValue);
    }
}