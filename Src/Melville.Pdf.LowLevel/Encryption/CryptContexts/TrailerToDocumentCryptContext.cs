using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.CryptContexts;

internal static class TrailerToDocumentCryptContext
{
    public static async ValueTask<IDocumentCryptContext> CreateCryptContextAsync(
        PdfDictionary trailer, string? userPassword)
    {
        var securityHandler = await SecurityHandlerFromTrailerAsync(trailer).CA();
        var key = securityHandler.TryComputeRootKey(userPassword??"", PasswordType.User);
        return key is null ?
            throw new ArgumentException("Incorrect user key for encryption"):
            securityHandler.CreateCryptContext(key);
    }

    private static async ValueTask<ISecurityHandler> SecurityHandlerFromTrailerAsync(PdfDictionary trailer) =>
        await trailer.GetOrNullAsync(KnownNames.Encrypt).CA() is not PdfDictionary dict
            ? NullSecurityHandler.Instance
            : await SecurityHandlerFactory.CreateSecurityHandlerAsync(trailer, dict).CA();

    public static async ValueTask<IDocumentCryptContext> CreateDecryptorFactoryAsync(
        PdfDictionary trailer, IPasswordSource passwordSource) =>
        await (await SecurityHandlerFromTrailerAsync(trailer).CA()).InteractiveGetCryptContextAsync(passwordSource).CA();
}