using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Encryption.Readers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public interface ILowLevelDocumentEncryptor
    {
        public PdfDictionary CreateEncryptionDictioanry(PdfArray id);
        byte[] UserPassword { get; }
    }

    public class DocumentEncryptorV3Rc4128 : ILowLevelDocumentEncryptor
    {
        private readonly byte[] ownerPassword;
        public byte[] UserPassword { get; set; }
        private readonly int pdfPermissionsValue;
        private readonly IComputeOwnerPassword ownerPasswordComputer;
        private readonly IComputeUserPassword userPasswordComputer;
        private readonly IEncryptionKeyComputer keyComputer;

        public DocumentEncryptorV3Rc4128(
            string userPassword, string ownerPassword, PdfPermission restrictedPermissions)
        {
            this.ownerPassword = ownerPassword.AsExtendedAsciiBytes();
            this.UserPassword = userPassword.AsExtendedAsciiBytes();
            pdfPermissionsValue = ~(int)restrictedPermissions;
            userPasswordComputer = new ComputeUserPasswordV3();
            ownerPasswordComputer = new ComputeOwnerPasswordV3();
            keyComputer = new EncryptionKeyComputerV3();
        }

        public PdfDictionary CreateEncryptionDictioanry(PdfArray id)
        {
            var ownerHash = ownerPasswordComputer.ComputeOwnerKey(ownerPassword,UserPassword, 16);
            var ep = new EncryptionParameters(
                ((PdfString) id.RawItems[0]).Bytes, ownerHash, Array.Empty<byte>(),
                (uint)pdfPermissionsValue, 128);
            return new PdfDictionary(
                (KnownNames.Filter, KnownNames.Standard),
                (KnownNames.V, new PdfInteger(2)),
                (KnownNames.Length, new PdfInteger(128)),
                (KnownNames.P, new PdfInteger(pdfPermissionsValue)),
                (KnownNames.R, new PdfInteger(3)),
                (KnownNames.U, new PdfString(UserHashForPassword(UserPassword, ep))),
                (KnownNames.O, new PdfString(
                    ownerHash))
            );
        }
        
        public byte[] UserHashForPassword(
            in ReadOnlySpan<byte> userPassword, in EncryptionParameters parameters)
        {
            var key = keyComputer.ComputeKey(userPassword, parameters);
            return userPasswordComputer.ComputeHash(key, parameters);
        }

    }
}