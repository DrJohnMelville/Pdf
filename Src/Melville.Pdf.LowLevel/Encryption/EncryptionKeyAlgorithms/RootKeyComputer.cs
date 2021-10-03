using System;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms
{
    public class RootKeyComputer
    {
        private readonly IGlobalEncryptionKeyComputer keyComputer;
        private readonly IComputeUserPassword userHashComputer;
        private readonly IComputeOwnerPassword ownerHashComputer;
        private readonly EncryptionParameters parameters;

        public RootKeyComputer(IGlobalEncryptionKeyComputer keyComputer, IComputeUserPassword userHashComputer, IComputeOwnerPassword ownerHashComputer, EncryptionParameters parameters)
        {
            this.keyComputer = keyComputer;
            this.userHashComputer = userHashComputer;
            this.ownerHashComputer = ownerHashComputer;
            this.parameters = parameters;
        }

        private byte[]? KeyFromUserPassword(in ReadOnlySpan<byte> userPassword)
        {
            var key = keyComputer.ComputeKey(userPassword, parameters);
            var userHash = userHashComputer.ComputeHash(key, parameters);
            var matches = userHashComputer.CompareHashes(userHash, parameters.UserPasswordHash);
            return matches ? key : null;
        }

        public byte[]? TryComputeRootKey(in ReadOnlySpan<byte> password, PasswordType type)
        {
            if (type == PasswordType.Owner)
                return TryComputeRootKey(ownerHashComputer.UserKeyFromOwnerKey(password, parameters), PasswordType.User);
            return KeyFromUserPassword(password);

        }
        
    }
}