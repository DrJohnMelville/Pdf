using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Encryption.Readers
{
    public interface ISecurityHandler
    {
        IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfName cryptFilterName);
        IObjectEncryptor EncryptorForObject(PdfIndirectObject parent, PdfName cryptFilterName);
        bool TrySinglePassword((string?, PasswordType) password);
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
            while (true)
            {
                var password = await passwordSource.GetPassword();
                if (handler.TrySinglePassword(password)) return;
            }
        }

    }

    public class SecurityHandler : ISecurityHandler
    {
        private readonly EncryptionParameters parameters;
        private readonly IEncryptionKeyComputer keyComputer;
        private readonly IComputeUserPassword userHashComputer;
        private readonly IComputeOwnerPassword ownerHashComputer;
        private readonly IEncryptorAndDecryptorFactory encryptorAndDecryptorFactory;
        private byte[]? encryptionKey;
        
        public SecurityHandler(
            EncryptionParameters parameters, 
            IEncryptionKeyComputer keyComputer, 
            IComputeUserPassword userHashComputer,
            IComputeOwnerPassword ownerHashComputer,
            IEncryptorAndDecryptorFactory encryptorAndDecryptorFactory)
        {
            this.parameters = parameters;
            this.keyComputer = keyComputer;
            this.userHashComputer = userHashComputer;
            this.ownerHashComputer = ownerHashComputer;
            this.encryptorAndDecryptorFactory = encryptorAndDecryptorFactory;
        }

        private bool TryUserPassword(in ReadOnlySpan<byte> password)
        {
            var key = keyComputer.ComputeKey(password, parameters);
            var userHash = userHashComputer.ComputeHash(key, parameters);
            if ((userHashComputer.CompareHashes(userHash, parameters.UserPasswordHash)))
            {
                encryptionKey = key;
                return true;
            }
            return false;
        }
        
        public bool TrySinglePassword((string?, PasswordType) password)
        {
            var (passwordText, type) = password;
            if (passwordText == null)
                throw new PdfSecurityException("User cancelled pdf decryption by not providing password.");
            switch (type)
            {
                case  PasswordType.Owner:
                    var userPassword = ownerHashComputer.UserKeyFromOwnerKey(passwordText.AsExtendedAsciiBytes(), parameters);
                    return TrySinglePassword(userPassword, PasswordType.User);
                case PasswordType.User:
                    if (TryUserPassword(passwordText.AsExtendedAsciiBytes())) return true;
                    break;
            }

            return false;
        }

        private bool TrySinglePassword(in ReadOnlySpan<byte> password, PasswordType type)
        {
            if (type == PasswordType.Owner)
                return TrySinglePassword(ownerHashComputer.UserKeyFromOwnerKey(password, parameters), PasswordType.User);
            return TryUserPassword(password);

        }

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

        public IObjectEncryptor EncryptorForObject(PdfIndirectObject parent, PdfName cryptFilterName)
        {
            VerifyEncryptionKeyExists();
            return encryptorAndDecryptorFactory.CreateEncryptor(
                encryptionKey, parent.ObjectNumber, parent.GenerationNumber);
        }
    }
}