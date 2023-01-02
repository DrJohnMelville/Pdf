using System;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

internal partial class V6Encryptor : ILowLevelDocumentEncryptor
{
    [FromConstructor] public string UserPassword { get; set; }
    [FromConstructor] private readonly string ownerPassword;
    [FromConstructor] private readonly PdfPermission permissionsRestricted;
    [FromConstructor] private readonly PdfName? defStream;
    [FromConstructor] private readonly PdfName? defString;
    [FromConstructor] private readonly PdfName? defEmbeddedFile;
    [FromConstructor] private readonly V4CfDictionary dictionary;
    [FromConstructor] private readonly IRandomNumberSource random;
    private readonly V6Cryptography crypto = new();

    public V6Encryptor(
        string userPassword, string ownerPassword, PdfPermission permissionsRestricted, PdfName? defStream,
        PdfName? defString, PdfName? defEmbeddedFile, V4CfDictionary dictionary) :
        this(userPassword, ownerPassword, permissionsRestricted, defStream, defString, defEmbeddedFile,
            dictionary, RandomNumberSource.Instance)
    {
    }

    public PdfDictionary CreateEncryptionDictionary(PdfArray id)
    {
        Span<byte> encryptionKey = stackalloc byte[32];
        random.Fill(encryptionKey);
        var userKey = ComputeGenericKey(UserPassword, Span<byte>.Empty, encryptionKey);
        var ownerKey = ComputeGenericKey(ownerPassword, userKey.WholeKey, encryptionKey);
        return new DictionaryBuilder()
            .WithItem(KnownNames.Filter, KnownNames.Standard)
            .WithItem(KnownNames.V, 5)
            .WithItem(KnownNames.R, 6)
            .WithItem(KnownNames.Length, 256)
            .WithItem(KnownNames.U, userKey.HashAsPdfString())
            .WithItem(KnownNames.UE, userKey.EncodedKeyAsPdfString())
            .WithItem(KnownNames.O, ownerKey.HashAsPdfString())
            .WithItem(KnownNames.OE, ownerKey.EncodedKeyAsPdfString())
            .WithItem(KnownNames.P, (int)permissionsRestricted)
            .WithItem(KnownNames.Perms, ComputePerms(encryptionKey))
            .WithItem(KnownNames.CF, dictionary.Build())
            .WithItem(KnownNames.StmF, defStream)
            .WithItem(KnownNames.StrF, defString)
            .WithItem(KnownNames.EFF, defEmbeddedFile)
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
    
    private PdfString ComputePerms(Span<byte> encryptionKey)
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
        return new PdfString(perms);
    }
}