using System;
using System.Buffers;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface ISecurityHandler
    {
        IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfObject target);
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
        private readonly IDecryptorFactory decryptorFactory;
        private byte[]? encryptionKey;
        
        public SecurityHandler(
            EncryptionParameters parameters, 
            IEncryptionKeyComputer keyComputer, 
            IComputeUserPassword userHashComputer,
            IComputeOwnerPassword ownerHashComputer,
            IDecryptorFactory decryptorFactory)
        {
            this.parameters = parameters;
            this.keyComputer = keyComputer;
            this.userHashComputer = userHashComputer;
            this.ownerHashComputer = ownerHashComputer;
            this.decryptorFactory = decryptorFactory;
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

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfObject target)
        {
            if (encryptionKey is null)
                throw new PdfSecurityException("No decryption key.  Call TryInteractiveLogin before decrypting.");
            return decryptorFactory.CreateDecryptor(encryptionKey, objectNumber, generationNumber);
        }
    }
}