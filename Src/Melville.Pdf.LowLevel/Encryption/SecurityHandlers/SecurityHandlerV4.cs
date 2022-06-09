using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.SecurityHandlers;

public class DocumentCryptContextV4: IDocumentCryptContext
{
    private Dictionary<PdfName, IDocumentCryptContext> contexts;
        

    public DocumentCryptContextV4(Dictionary<PdfName, IDocumentCryptContext> contexts)
    {
        this.contexts = contexts;
    }

    public IObjectCryptContext ContextForObject(int objectNumber, int generationNumber) =>
        new ObjectContextV4(contexts, objectNumber, generationNumber);

    public bool BlockEncryption(PdfObject item) => contexts.Values.Any(i => i.BlockEncryption(item));
}
public class ObjectContextV4: IObjectCryptContext
{
    private readonly IReadOnlyDictionary<PdfName, IDocumentCryptContext> context;
    private readonly int objectNumber;
    private readonly int genetrationNumber;

    public ObjectContextV4(IReadOnlyDictionary<PdfName, IDocumentCryptContext> context, int objectNumber, int genetrationNumber)
    {
        this.context = context;
        this.objectNumber = objectNumber;
        this.genetrationNumber = genetrationNumber;
    }

    public ICipher StringCipher() =>
        context[KnownNames.StrF]
            .ContextForObject(objectNumber, genetrationNumber)
            .StringCipher();

    public ICipher StreamCipher()=>
        context[KnownNames.StmF]
            .ContextForObject(objectNumber, genetrationNumber)
            .StringCipher();

    public ICipher NamedCipher(PdfName name) =>
        context[name]
            .ContextForObject(objectNumber, genetrationNumber)
            .NamedCipher(name);
}

public class SecurityHandlerV4 : ISecurityHandler
{
    private readonly Dictionary<PdfName, ISecurityHandler> handlers;
    private readonly RootKeyComputer rootKeyComputer;

    public SecurityHandlerV4(RootKeyComputer rootKeyComputer, Dictionary<PdfName, ISecurityHandler> handlers)
    {
        this.rootKeyComputer = rootKeyComputer;
        this.handlers = handlers;
    }

    public byte[]? TryComputeRootKey(string password, PasswordType type)
    {
        return rootKeyComputer.TryComputeRootKey(password.AsExtendedAsciiBytes(), type);
    }
    public IDocumentCryptContext CreateCryptContext(byte[] rootKey) =>
        new DocumentCryptContextV4(handlers.ToDictionary(
            handlerPair => handlerPair.Key,
            handlerPair => handlerPair.Value.CreateCryptContext(rootKey)));
}