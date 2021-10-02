using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.New;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public interface ISecurityHandler
    {
        [Obsolete("Switch to new model")]
        IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName cryptFilterName);
        [Obsolete("Switch to new Mode")]
        IObjectEncryptor EncryptorForObject(int objNum, int generationNumber);
        [Obsolete("Switch to new Mode")]
        bool TrySinglePassword((string?, PasswordType) password);
        byte[]? TryComputeRootKey(string password, PasswordType type);
        IDocumentCryptContext CreateCryptContext(byte[] rootKey);
    }

    public static class SecurityHandlerOperations
    {
        public static ValueTask TryInteactiveLogin(
            this ISecurityHandler handler, IPasswordSource passwordSource) => 
            handler.TrySinglePassword(("",PasswordType.User)) ? 
                new ValueTask() : 
                InnerTryInteractiveLogin(handler,passwordSource);

        private static async ValueTask InnerTryInteractiveLogin(
            ISecurityHandler handler, IPasswordSource passwordSource)
        {
            if (handler.TrySinglePassword(("", PasswordType.User))) return;
            while (true)
            {
                var password = await passwordSource.GetPassword();
                if (handler.TrySinglePassword(password)) return;
            }
        }

        public static async ValueTask<IDocumentCryptContext> InteractiveGetCryptContext(
            this ISecurityHandler handler, IPasswordSource source)
        {
            while (true)
            {
                var (password, type) = await source.GetPassword();
                if (password == null)
                    throw new PdfSecurityException("User cancelled pdf decryption by not providing password.");
                if (handler.TryComputeRootKey(password, type) is { } rootKey) 
                    return handler.CreateCryptContext(rootKey);
            }
        }
    }

    public class SecurityHandler : ISecurityHandler
    {
        private readonly IEncryptorAndDecryptorFactory encryptorAndDecryptorFactory;
        private readonly IKeySpecializer keySpecializer;
        private readonly ICipherFactory cipherFactory;
        private readonly RootKeyComputer rootKeyComputer;
        private byte[]? encryptionKey;
        private PdfObject? blockEncryption;
        
        public SecurityHandler(IEncryptorAndDecryptorFactory encryptorAndDecryptorFactory, 
            IKeySpecializer keySpecializer, 
            ICipherFactory cipherFactory,
            RootKeyComputer rootKeyComputer, PdfObject? blockEncryption)
        {
            this.encryptorAndDecryptorFactory = encryptorAndDecryptorFactory;
            this.keySpecializer = keySpecializer;
            this.cipherFactory = cipherFactory;
            this.rootKeyComputer = rootKeyComputer;
            this.blockEncryption = blockEncryption;
        }

        public byte[]? TryComputeRootKey(string password, PasswordType type) => 
            encryptionKey = rootKeyComputer.TryComputeRootKey(password.AsExtendedAsciiBytes(), type);

        public IDocumentCryptContext CreateCryptContext(byte[] rootKey) => 
            new DocumentCryptContext(rootKey, keySpecializer, cipherFactory, blockEncryption);

        public bool TrySinglePassword((string?, PasswordType) password)
        {
            var (passwordText, type) = password;
            if (passwordText == null) 
                throw new PdfSecurityException("User cancelled pdf decryption by not providing password.");
            return TryComputeRootKey(passwordText, type) != null;
        }

        [Obsolete]
        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName? cryptFilterName)
        {
            VerifyEncryptionKeyExists();
            return encryptorAndDecryptorFactory.CreateDecryptor(encryptionKey, objectNumber, generationNumber);
        }

        [MemberNotNull(nameof(encryptionKey))]
        private void VerifyEncryptionKeyExists()
        {
            if (encryptionKey is null)
                throw new PdfSecurityException("No decryption key.  Call TryInteractiveLogin before decrypting.");
        }

        [Obsolete]
        public IObjectEncryptor EncryptorForObject(int objNum, int generationNumber)
        {
            VerifyEncryptionKeyExists();
            return encryptorAndDecryptorFactory.CreateEncryptor(
                encryptionKey, objNum, generationNumber);
        }
    }
}