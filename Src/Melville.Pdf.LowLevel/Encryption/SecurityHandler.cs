using System;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.Decryptors;

namespace Melville.Pdf.LowLevel.Encryption
{
    public interface ISecurityHandler
    {
        bool TyyUserPassword(in Span<byte> password);
        IDecryptor DecryptorForObject(int objectNumber, int generationNumber);
    }

    public static class SecurityHandlerOperations
    {
        public static bool TryUserPassword(this ISecurityHandler handler, string password) =>
          handler.TyyUserPassword(password.AsExtendedAsciiBytes());
    }

    public class SecurityHandler : ISecurityHandler
    {
        private readonly EncryptionParameters Parameters;
        private readonly IEncryptionKeyComputer keyComputer;
        private readonly IComputeUserPassword userHashComputer;
        private byte[]? encryptionKey;
        
        public SecurityHandler(
            EncryptionParameters parameters, 
            IEncryptionKeyComputer keyComputer, 
            IComputeUserPassword userHashComputer)
        {
            Parameters = parameters;
            this.keyComputer = keyComputer;
            this.userHashComputer = userHashComputer;
        }

        public bool TyyUserPassword(in Span<byte> password)
        {
            var key = keyComputer.ComputeKey(password, Parameters);
            var userHash = userHashComputer.ComputeHash(key, Parameters);
            if ((userHashComputer.CompareHashes(userHash, Parameters.UserPasswordHash)))
            {
                encryptionKey = key;
                return true;
            }
            return false;
        }

        public IDecryptor DecryptorForObject(int objectNumber, int generationNumber)
        {
            throw new NotImplementedException();
        }
    }
}