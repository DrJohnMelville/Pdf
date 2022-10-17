using System;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Encryption.PasswordHashes;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public class V6Encryptor: EncryptorWithCfsDictionary
{
    public V6Encryptor(
        string userPassword, string ownerPassword, PdfPermission permissionsRestricted,
        PdfName defStream, PdfName defString, PdfName defEmbeddedFile) : 
        base(userPassword, ownerPassword, 5, 6, -1, permissionsRestricted, 
            ComputeOwnerPasswordV3.Instance, new ComputeUserPasswordV3(), new GlobalEncryptionKeyComputerV3(),
            defStream, defString, defEmbeddedFile, 
            new V4CfDictionary(KnownNames.AESV3, 256))
    {
    }
}

public class ComputeOwnerPasswordv6 : IComputeOwnerPassword
{
    public string UserKeyFromOwnerKey(string ownerKey, EncryptionParameters parameters)
    {
        throw new NotImplementedException();
    }

    public byte[] ComputeOwnerKey(string ownerKey, string userKey, int keyLenInBytes)
    {
        throw new NotImplementedException();
    }
}