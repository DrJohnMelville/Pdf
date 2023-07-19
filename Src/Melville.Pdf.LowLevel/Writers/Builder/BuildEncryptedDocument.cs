using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Writers.Builder;

/// <summary>
/// Extension method that adds encryption to an IPdfObjectRegistry.
/// </summary>
public static class BuildEncryptedDocument
{
    /// <summary>
    /// Add encryption to an IPdfObjectRegistry
    /// </summary>
    /// <param name="builder">IPdfObjectRegistry that should write an encrypted document.</param>
    /// <param name="encryptor">The encryptor to use in writing the document.</param>
    public static void AddEncryption(
        this ILowLevelDocumentCreator builder,ILowLevelDocumentEncryptor encryptor)
    {
        builder.AddToTrailerDictionary(
            KnownNames.EncryptTName,
            builder.Add(
                encryptor.CreateEncryptionDictionary(builder.EnsureDocumentHasId())));
    }
        
}