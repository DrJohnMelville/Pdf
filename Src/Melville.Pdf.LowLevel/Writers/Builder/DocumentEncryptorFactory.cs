using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Create the ILowLevelDocumentEncryptor for various encryption algorithms.
/// </summary>
public static class DocumentEncryptorFactory
{
    /// <summary>
    /// Create a document encryptor using the Version 2 128 bit RC4 cipher.
    /// </summary>
    /// <param name="userPassword">Password to use document according tor restrictions flag.</param>
    /// <param name="ownerPassword">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V2R3Rc4128(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
        R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 2, 128);

    /// <summary>
    /// Create a document encryptor using the Version 2 40 bit RC4 cipher.
    /// </summary>
    /// <param name="userPassword">Password to use document according tor restrictions flag.</param>
    /// <param name="ownerPassword">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V1R2Rc440(
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

    /// <summary>
    /// Create a Document Encryptor using the V3 algorithm
    /// </summary>
    /// <param name="userPassword">Password to use document according tor restrictions flag.</param>
    /// <param name="ownerPassword">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <param name="V">Cryptosystem to use</param>
    /// <param name="keyLengthInBits">Length of the primary document key in bits.</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    private static ComputeEncryptionDictionary R3Encryptor(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions,
        int V, int keyLengthInBits) =>
        new(userPassword, ownerPassword,
            V, 3, keyLengthInBits, restrictedPermissions,
            ComputeOwnerPasswordV3.Instance,
            new ComputeUserPasswordV3(),
            new GlobalEncryptionKeyComputerV3());

    /// <summary>
    /// Create a Document Encryptor using the V2 R E Rc4 40 bit algorithm
    /// </summary>
    /// <param name="userPassword">Password to use document according tor restrictions flag.</param>
    /// <param name="ownerPassword">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V2R3Rc440(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
        R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 2, 40);

    /// <summary>
    /// Create a document encryptor using the Version 1 40 bit RC4 cipher.
    /// </summary>
    /// <param name="userPassword">Password to use document according tor restrictions flag.</param>
    /// <param name="ownerPassword">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V1R3Rc440(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions) =>
        R3Encryptor(userPassword, ownerPassword, restrictedPermissions, 1, 40);

    /// <summary>
    /// Create a document encryptor using the V4 encryption algorithms.
    /// </summary>
    /// <param name="userPassword">Password to use document according tor restrictions flag.</param>
    /// <param name="ownerPassword">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <param name="encryptor">Desired encryption algorithm</param>
    /// <param name="keyLengthInBytes">Length of the primary document key in bytes</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V4(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions,
        EncryptorName encryptor, int keyLengthInBytes) =>
        V4(userPassword, ownerPassword, restrictedPermissions, 
            KnownNames.StdCF, KnownNames.StdCF, KnownNames.StmF, 
            new V4CfDictionary(encryptor, keyLengthInBytes));

    /// <summary>
    /// Create a document encryptor using the V4 encryption algorithms.
    /// </summary>
    /// <param name="userPassword">Password to use document according tor restrictions flag.</param>
    /// <param name="ownerPassword">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <param name="streamEnc">Name of the encryptor for streams.</param>
    /// <param name="stringEnc">Name of the encryptor for strings</param>
    /// <param name="embededFileEnc">Name of the encryptor for embedded files.</param>
    /// <param name="encryptors">A set of custom encryptors</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V4(
        string userPassword, string ownerPassword, PdfPermission restrictedPermissions,
        PdfName streamEnc, PdfName stringEnc, PdfName embededFileEnc, in V4CfDictionary encryptors) =>
        new V4Encryptor(userPassword, ownerPassword, 128, restrictedPermissions,
            streamEnc, stringEnc, embededFileEnc, encryptors);

    /// <summary>
    /// Create a document encryptor using the V6 encryption algorithms.
    /// </summary>
    /// <param name="user">Password to use document according tor restrictions flag.</param>
    /// <param name="owner">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V6(
        string user, string owner, PdfPermission restrictedPermissions) => V6(
        user, owner, restrictedPermissions, KnownNames.StdCF, KnownNames.StdCF, KnownNames.StmF);

    /// <summary>
    /// Create a document encryptor using the V6 encryption algorithms.
    /// </summary>
    /// <param name="user">Password to use document according tor restrictions flag.</param>
    /// <param name="owner">Password for unlimited access to document.</param>
    /// <param name="restrictedPermissions">Privileges that should be DENIED to uses with a user password.</param>
    /// <param name="streamEnc">Name of the encryptor for streams.</param>
    /// <param name="stringEnc">Name of the encryptor for strings</param>
    /// <param name="embededFileEnc">Name of the encryptor for embedded files.</param>
    /// <param name="dictionary">A set of custon encryptors</param>
    /// <returns>An ILowLevelDocumentEncryptor which can be used to make an LowLevelDocumentWriter write an encrypted document.</returns>
    public static ILowLevelDocumentEncryptor V6(
        string user, string owner, PdfPermission restrictedPermissions,
        PdfName streamEnc, PdfName stringEnc, PdfName embededFileEnc, V4CfDictionary? dictionary = null) =>
        new EncryptionV6.V6Encryptor(user, owner, restrictedPermissions,
            streamEnc, stringEnc, embededFileEnc,
             dictionary ?? new V4CfDictionary(KnownNames.AESV3, 32, KnownNames.DocOpen));
}