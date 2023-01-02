using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public static class BuildEncryptedDocument
{
    /// <summary>
    /// Add encryption to an ILowLevelDocumentBuilder
    /// </summary>
    /// <param name="builder">ILowLevelDocumentBuilder that should write an encrypted document.</param>
    /// <param name="encryptor">The encryptor to use in writing the document.</param>
    public static void AddEncryption(
        this ILowLevelDocumentBuilder builder,ILowLevelDocumentEncryptor encryptor)
    {
        builder.UserPassword = encryptor.UserPassword;
        builder.AddToTrailerDictionary(
            KnownNames.Encrypt,
            builder.Add(
                encryptor.CreateEncryptionDictionary(builder.EnsureDocumentHasId())));
    }
        
}