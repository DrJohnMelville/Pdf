using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface ISecurityHandler
    {
        IDecryptor DecryptorForObject(int objectNumber, int generationNumber);
        ValueTask TryInteactiveLogin(IPasswordSource passwordSource);
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
            IComputeUserPassword userHashComputer)
        {
            this.parameters = parameters;
            this.keyComputer = keyComputer;
            this.userHashComputer = userHashComputer;
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

        public ValueTask TryInteactiveLogin(IPasswordSource passwordSource) => 
            TyyUserPassword(Array.Empty<byte>()) ? 
                new ValueTask() : 
                InnerTryInteractviceLogin(passwordSource);

        private async ValueTask InnerTryInteractviceLogin(IPasswordSource passwordSource)
        {
            while (true)
            {
                switch (await passwordSource.GetPassword())
                {
                    case (null, _):
                        throw new PdfSecurityException("User cancelled pdf decryption by not providing password.");
                    case (_, PasswordType.Owner):
                        throw new NotImplementedException("Pdf Owner Password is not implemented yet.");
                    case (var s, PasswordType.User):
                        if (TyyUserPassword(s.AsExtendedAsciiBytes())) return;
                        break;
                }
            }
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber)
        {
            if (encryptionKey is null)
                throw new PdfSecurityException("No decryption key.  Call TryUserPassword before decrypting.");
            return decryptorFactory.CreateDecryptor(encryptionKey, objectNumber, generationNumber);
        }
    }
}