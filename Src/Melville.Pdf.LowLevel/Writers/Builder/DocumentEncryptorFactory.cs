using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public static class DocumentEncryptorFactory
{
    public static ILowLevelDocumentEncryptor V2R3Rc4128(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
        R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 2, 128);

    private static ComputeEncryptionDictionary R3Encryptor(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions, 
        int V, int keyLengthInBits) =>
        new(userPassword, ownerPassword,
            V, 3, keyLengthInBits, restrictedPermissions,
            new ComputeOwnerPasswordV3(),
            new ComputeUserPasswordV3(),
            new GlobalEncryptionKeyComputerV3());

    public static ILowLevelDocumentEncryptor v1R2Rc440(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
        R2Encryptor(userPassword, ownerPassword, restrictedPermissions, 1, 40);

    private static ComputeEncryptionDictionary R2Encryptor(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions, 
        int V, int keyLengthInBits) =>
        new(userPassword, ownerPassword,
            V, 2, keyLengthInBits, restrictedPermissions,
            new ComputeOwnerPasswordV2(),
            new ComputeUserPasswordV2(),
            new GlobalEncryptionKeyComputerV2());

    public static ILowLevelDocumentEncryptor V2R3Rc440(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
        R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 2, 40);

    public static ILowLevelDocumentEncryptor V1R3Rc440(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
        R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 1, 40);


    public static ILowLevelDocumentEncryptor V4(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions, 
        PdfName encryptor, int keyLengthInBytes) =>
        V4(userPassword, ownerPassword, restrictedPermissions, 
            KnownNames.StdCF, KnownNames.StdCF, new V4CfDictionary(encryptor, keyLengthInBytes));

    public static ILowLevelDocumentEncryptor V4(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions,
        PdfName streamEnc, PdfName stringEnc, in V4CfDictionary encryptors)
    {
        return new V4Encryptor(userPassword, ownerPassword, 128, restrictedPermissions,
            streamEnc, stringEnc, encryptors);
    }
}