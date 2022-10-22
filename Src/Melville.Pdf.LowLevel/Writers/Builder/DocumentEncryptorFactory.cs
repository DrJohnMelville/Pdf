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
            ComputeOwnerPasswordV3.Instance,
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
            KnownNames.StdCF, KnownNames.StdCF, KnownNames.StmF, 
            new V4CfDictionary(encryptor, keyLengthInBytes));

    public static ILowLevelDocumentEncryptor V4(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions,
        PdfName streamEnc, PdfName stringEnc, PdfName embededFileEnc, in V4CfDictionary encryptors) =>
        new V4Encryptor(userPassword, ownerPassword, 128, restrictedPermissions,
            streamEnc, stringEnc, embededFileEnc, encryptors);

    public static ILowLevelDocumentEncryptor V6(
        string user, string owner, PdfPermission restrictedPermissions) => V6(
        user, owner, restrictedPermissions, KnownNames.StmF, KnownNames.StrF, KnownNames.StmF);
    public static ILowLevelDocumentEncryptor V6(
        string user, string owner, PdfPermission restrictedPermissions,
        PdfName streamEnc, PdfName stringEnc, PdfName embededFileEnc, V4CfDictionary? dictionary = null) =>
        new V6Encryptor(user, owner, restrictedPermissions,
            streamEnc, stringEnc, embededFileEnc,
             dictionary ?? new V4CfDictionary(KnownNames.AESV3, 32, KnownNames.DocOpen));
}