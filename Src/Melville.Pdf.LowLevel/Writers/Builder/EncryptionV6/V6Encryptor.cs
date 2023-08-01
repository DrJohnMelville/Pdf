using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

internal partial class V6Encryptor : ILowLevelDocumentEncryptor
{
    [FromConstructor] public string UserPassword { get; set; }
    [FromConstructor] private readonly string ownerPassword;
    [FromConstructor] private readonly PdfPermission permissionsRestricted;
    [FromConstructor] private readonly PdfDirectValue defStream;
    [FromConstructor] private readonly PdfDirectValue defString;
    [FromConstructor] private readonly PdfDirectValue defEmbeddedFile;
    [FromConstructor] private readonly V4CfDictionary dictionary;
    [FromConstructor] private readonly IRandomNumberSource random;
    private readonly V6Cryptography crypto = new();

    public V6Encryptor(
        string userPassword, string ownerPassword, PdfPermission permissionsRestricted, PdfDirectValue defStream,
        PdfDirectValue defString, PdfDirectValue defEmbeddedFile, V4CfDictionary dictionary) :
        this(userPassword, ownerPassword, permissionsRestricted, defStream, defString, defEmbeddedFile,
            dictionary, RandomNumberSource.Instance)
    {
    }

    public PdfValueDictionary CreateEncryptionDictionary(PdfValueArray id)
    {
        Span<byte> encryptionKey = stackalloc byte[32];
        random.Fill(encryptionKey);
        var userKey = ComputeGenericKey(UserPassword, Span<byte>.Empty, encryptionKey);
        var ownerKey = ComputeGenericKey(ownerPassword, userKey.WholeKey, encryptionKey);
        return new ValueDictionaryBuilder()
            .WithItem(KnownNames.FilterTName, KnownNames.StandardTName)
            .WithItem(KnownNames.VTName, 5)
            .WithItem(KnownNames.RTName, 6)
            .WithItem(KnownNames.LengthTName, 256)
            .WithItem(KnownNames.UTName, userKey.HashAsPdfString())
            .WithItem(KnownNames.UETName, userKey.EncodedKeyAsPdfString())
            .WithItem(KnownNames.OTName, ownerKey.HashAsPdfString())
            .WithItem(KnownNames.OETName, ownerKey.EncodedKeyAsPdfString())
            .WithItem(KnownNames.PTName, (int)permissionsRestricted)
            .WithItem(KnownNames.PermsTName, ComputePerms(encryptionKey))
            .WithItem(KnownNames.CFTName, dictionary.Build())
            .WithItem(KnownNames.StmFTName, defStream)
            .WithItem(KnownNames.StrFTName, defString)
            .WithItem(KnownNames.EFFTName, defEmbeddedFile)
            .AsDictionary();
    }
    
    private V6EncryptionKey ComputeGenericKey(
        string password, in Span<byte> extraBytes, in Span<byte> encryptionKey)
    {
        var key = V6EncryptionKey.FromRandomSource(random);
        HashAlgorithm2B.ComputePasswordHash(password, key.ValidationSalt, extraBytes, key.Hash, crypto);
        EncryptKey(password, key, extraBytes,encryptionKey);
        return key;
        
    }

    private void EncryptKey(
        string password, in V6EncryptionKey key, Span<byte> extraBytes, Span<byte> encryptionKey)
    {
        Span<byte> intermediateKey = stackalloc byte[32];
        HashAlgorithm2B.ComputePasswordHash(password, key.KeySalt, extraBytes, intermediateKey, crypto);
        crypto.Cbc.Encrypt(intermediateKey, stackalloc byte[16], encryptionKey, key.EncryptedFileKey);
    }
    
    private PdfDirectValue ComputePerms(Span<byte> encryptionKey)
    {
        Span<byte> source = stackalloc byte[]
        {
            (byte)permissionsRestricted,
            (byte)((int)permissionsRestricted >> 8),
            (byte)((int)permissionsRestricted >> 16),
            (byte)((int)permissionsRestricted >> 24),
            0xFF, 0xFF, 0xFF, 0xFF,
            0x54, // need to replace this with the encryptmetadata flag
            0x61, 0x64, 0x62,
            0, 0, 0, 0
        };
        random.Fill(source[^4..]);
        var perms = new byte[16];
        crypto.Ecb.Encrypt(encryptionKey, Span<byte>.Empty, source, perms);
        return PdfDirectValue.CreateString(perms);
    }
}