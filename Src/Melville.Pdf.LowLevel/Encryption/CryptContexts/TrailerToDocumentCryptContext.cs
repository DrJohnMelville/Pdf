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
    public static async ValueTask<IDocumentCryptContext> CreateCryptContext(
        PdfDictionary trailer, string? userPassword)
    {
        var securityHandler = await SecurityHandlerFromTrailer(trailer).CA();
        var key = securityHandler.TryComputeRootKey(userPassword??"", PasswordType.User);
        return key is null ?
            throw new ArgumentException("Incorrect user key for encryption"):
            securityHandler.CreateCryptContext(key);
    }

    private static async ValueTask<ISecurityHandler> SecurityHandlerFromTrailer(PdfDictionary trailer) =>
        await trailer.GetOrNullAsync(KnownNames.Encrypt).CA() is not PdfDictionary dict
            ? NullSecurityHandler.Instance
            : await SecurityHandlerFactory.CreateSecurityHandler(trailer, dict).CA();

    public static async ValueTask<IDocumentCryptContext> CreateDecryptorFactory(
        PdfDictionary trailer, IPasswordSource passwordSource) =>
        await (await SecurityHandlerFromTrailer(trailer).CA()).InteractiveGetCryptContext(passwordSource).CA();
}