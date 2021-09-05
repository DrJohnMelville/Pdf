using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface ISecurityHandler
    {
        IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfObject target);
        bool TyySinglePassword((string?, PasswordType) password);
    }

    public static class SecurityHandlerOperations
    {
        public static ValueTask TryInteactiveLogin(
            this ISecurityHandler handler, IPasswordSource passwordSource) => 
            handler.TyySinglePassword(("",PasswordType.User)) ? 
                new ValueTask() : 
                InnerTryInteractiveLogin(handler,passwordSource);

        private static async ValueTask InnerTryInteractiveLogin(
            ISecurityHandler handler, IPasswordSource passwordSource)
        {
            while (true)
            {
                var password = await passwordSource.GetPassword();
                if (handler.TyySinglePassword(password)) return;
            }
        }

    }

    public class SecurityHandler : ISecurityHandler
    {
        private readonly EncryptionParameters parameters;
        private readonly IEncryptionKeyComputer keyComputer;
        private readonly IComputeUserPassword userHashComputer;
        private readonly IDecryptorFactory decryptorFactory;
        private byte[]? encryptionKey;
        
        public SecurityHandler(
            EncryptionParameters parameters, 
            IEncryptionKeyComputer keyComputer, 
            IComputeUserPassword userHashComputer,
            IDecryptorFactory decryptorFactory)
        {
            this.parameters = parameters;
            this.keyComputer = keyComputer;
            this.userHashComputer = userHashComputer;
            this.decryptorFactory = decryptorFactory;
        }

        private bool TyyUserPassword(in Span<byte> password)
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
        
        public bool TyySinglePassword((string?, PasswordType) password)
        {
            switch (password)
            {
                case (null, _):
                    throw new PdfSecurityException("User cancelled pdf decryption by not providing password.");
                case (_, PasswordType.Owner):
                    throw new NotImplementedException("Pdf Owner Password is not implemented yet.");
                case (var s, PasswordType.User):
                    if (TyyUserPassword(s.AsExtendedAsciiBytes())) return true;
                    break;
            }

            return false;
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber, PdfObject target)
        {
            if (encryptionKey is null)
                throw new PdfSecurityException("No decryption key.  Call TryInteractiveLogin before decrypting.");
            return decryptorFactory.CreateDecryptor(encryptionKey, objectNumber, generationNumber);
        }
    }
}