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
        public PdfDictionary CreateEncryptionDictionary(PdfArray id);
        public byte[] UserPassword { get; }
    }

    public class StandardDocumentEncryptor : ILowLevelDocumentEncryptor
    {
        private readonly byte[] ownerPassword;
        public byte[] UserPassword { get; set; }
        private readonly int permissions;
        private readonly IComputeOwnerPassword ownerPasswordComputer;
        private readonly IComputeUserPassword userPasswordComputer;
        private readonly IEncryptionKeyComputer keyComputer;
        private int v;
        private int r;
        private int keyLengthInBits;
        private int KeyLengthInBytes => keyLengthInBits / 8;

        public StandardDocumentEncryptor(
            string userPassword,
            string ownerPassword,
            int v,
            int r,
            int keyLengthInBits,
            PdfPermission permissionsRestricted, 
            IComputeOwnerPassword ownerPasswordComputer, 
            IComputeUserPassword userPasswordComputer, 
            IEncryptionKeyComputer keyComputer)
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
            var ownerHash = ownerPasswordComputer.ComputeOwnerKey(ownerPassword,UserPassword, KeyLengthInBytes);
            var ep = new EncryptionParameters(
                ((PdfString) id.RawItems[0]).Bytes, ownerHash, Array.Empty<byte>(),
                (uint)permissions, keyLengthInBits);
            return new PdfDictionary(
                (KnownNames.Filter, KnownNames.Standard),
                (KnownNames.V, new PdfInteger(v)),
                (KnownNames.Length, new PdfInteger(keyLengthInBits)),
                (KnownNames.P, new PdfInteger(permissions)),
                (KnownNames.R, new PdfInteger(r)),
                (KnownNames.U, new PdfString(UserHashForPassword(UserPassword, ep))),
                (KnownNames.O, new PdfString(
                    ownerHash))
            );
        }
        
        public byte[] UserHashForPassword(
            in ReadOnlySpan<byte> userPassword, in EncryptionParameters parameters)
        {
            var key = keyComputer.ComputeKey(userPassword, parameters);
            var ret = userPasswordComputer.ComputeHash(key, parameters);
            return ret;
        }
    }

    public static class DocumentEncryptorFactory
    {
        public static ILowLevelDocumentEncryptor V2R3Rc4128(
            string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
            R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 2, 128);

        private static StandardDocumentEncryptor R3Encryptor(
            string userPassword, string ownerPassword, PdfPermission restrictedPermissions, 
            int V, int keyLengthInBits) =>
            new(userPassword, ownerPassword,
                V, 3, keyLengthInBits, restrictedPermissions,
                new ComputeOwnerPasswordV3(),
                new ComputeUserPasswordV3(),
                new EncryptionKeyComputerV3());

        public static ILowLevelDocumentEncryptor v1R2Rc440(
            string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
            R2Encryptor(userPassword, ownerPassword, restrictedPermissions, 1, 40);

        private static StandardDocumentEncryptor R2Encryptor(
            string userPassword, string ownerPassword, PdfPermission restrictedPermissions, 
            int V, int keyLengthInBits) =>
            new(userPassword, ownerPassword,
                V, 2, keyLengthInBits, restrictedPermissions,
                new ComputeOwnerPasswordV2(),
                new ComputeUserPasswordV2(),
                new EncryptionKeyComputerV2());

        public static ILowLevelDocumentEncryptor V2R3Rc440(
            string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
            R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 2, 40);

        public static ILowLevelDocumentEncryptor V1R3Rc440(
            string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
            R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 1, 40);
    }
}