using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;

internal interface IRootKeyComputer
{
    byte[]? TryComputeRootKey(string password, PasswordType type);
}

internal class RootKeyComputer : IRootKeyComputer
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

    private byte[]? KeyFromUserPassword(in string userPassword)
    {
        var key = keyComputer.ComputeKey(userPassword, parameters);
        var userHash = userHashComputer.ComputeHash(key, parameters);
        var matches = userHashComputer.CompareHashes(userHash, parameters.UserPasswordHash.Span);
        return matches ? key : null;
    }

    public byte[]? TryComputeRootKey(string password, PasswordType type) => 
        type == PasswordType.Owner ? 
            TryComputeRootKey(ownerHashComputer.UserKeyFromOwnerKey(password, parameters), PasswordType.User) : 
            KeyFromUserPassword(password);
}