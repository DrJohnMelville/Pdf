using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

internal interface ISecurityHandler
{
    byte[]? TryComputeRootKey(string password, PasswordType type);
    IDocumentCryptContext CreateCryptContext(byte[] rootKey);
}

internal static class SecurityHandlerOperations
{
    public static async ValueTask<IDocumentCryptContext> InteractiveGetCryptContextAsync(
        this ISecurityHandler handler, IPasswordSource source)
    {
        return handler.CreateCryptContext(await GetRootKeyAsync(handler, source).CA());
    }

    private static ValueTask<byte[]> GetRootKeyAsync(ISecurityHandler handler, IPasswordSource source) =>
        handler.TryComputeRootKey("", PasswordType.User) is { } rootKey
            ? new(rootKey)
            : InteractiveGetRootKeyAsync(handler, source);
        
    private static async ValueTask<byte[]> InteractiveGetRootKeyAsync(
        ISecurityHandler handler, IPasswordSource source)
    {
        while (true)
        {
            var (password, type) = await source.GetPasswordAsync().CA();
            if (password == null)
                throw new PdfSecurityException("User cancelled pdf decryption by not providing password.");
            if (handler.TryComputeRootKey(password, type) is { } rootKey) 
                return rootKey;
        }
    }
}

internal partial class SecurityHandler : ISecurityHandler
{
    [FromConstructor]private readonly IKeySpecializer keySpecializer;
    [FromConstructor]private readonly ICipherFactory cipherFactory;
    [FromConstructor]private readonly IRootKeyComputer rootKeyComputer;
    [FromConstructor]private readonly PdfObject? blockEncryption;
        
    public byte[]? TryComputeRootKey(string password, PasswordType type) => 
        rootKeyComputer.TryComputeRootKey(password, type);

    public IDocumentCryptContext CreateCryptContext(byte[] rootKey) => 
        new DocumentCryptContext(rootKey, keySpecializer, cipherFactory, blockEncryption);
}