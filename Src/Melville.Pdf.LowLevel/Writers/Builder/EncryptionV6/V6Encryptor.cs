using System;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

public partial class V6Encryptor : ILowLevelDocumentEncryptor
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
        var userKey = ComputeUserKey();
        var ownerKey = ComputeOwnerKey(userKey);
        return new DictionaryBuilder()
            .WithItem(KnownNames.Filter, KnownNames.Standard)
            .WithItem(KnownNames.V, 5)
            .WithItem(KnownNames.R, 6)
            .WithItem(KnownNames.Length, 256)
            .WithItem(KnownNames.U, userKey.AsPdfString())
            .WithItem(KnownNames.UE, EncryptUserKey(userKey, encryptionKey))
            .WithItem(KnownNames.O, ownerKey.AsPdfString())
            .WithItem(KnownNames.OE, EncryptOwnerKey(userKey, ownerKey, encryptionKey))
            .WithItem(KnownNames.P, (int)permissionsRestricted)
            .WithItem(KnownNames.Perms, ComputePerms(encryptionKey))
            .AsDictionary();
    }
    
    private V6EncryptionKey ComputeUserKey() => ComputeGenericKey(UserPassword, Span<byte>.Empty);
    private V6EncryptionKey ComputeOwnerKey(in V6EncryptionKey userKey) => 
        ComputeGenericKey(ownerPassword, userKey.WholeKey);
    private V6EncryptionKey ComputeGenericKey(string password, in Span<byte> extraBytes)
    {
        var key = V6EncryptionKey.FromRandomSource(random);
        HashAlgorithm2B.ComputePasswordHash(password, key.ValidationSalt, extraBytes, key.Hash, crypto);
        return key;
        
    }
    private PdfString EncryptUserKey(in V6EncryptionKey userKey, Span<byte> encryptionKey) => 
        EncryptKey(userKey, Span<byte>.Empty, encryptionKey);

    private PdfString EncryptOwnerKey(
        in V6EncryptionKey userKey, in V6EncryptionKey ownerKey, Span<byte> encryptionKey) =>
        EncryptKey(ownerKey, userKey.WholeKey, encryptionKey);

    private PdfString EncryptKey(in V6EncryptionKey key, Span<byte> extraBytes, Span<byte> encryptionKey)
    {
        Span<byte> intermediateKey = stackalloc byte[32];
        HashAlgorithm2B.ComputePasswordHash(
            UserPassword, key.KeySalt, extraBytes, intermediateKey, crypto);
        var destination = new byte[32];
        crypto.Cbc.Encrypt(intermediateKey, stackalloc byte[16], encryptionKey, destination);
        return new PdfString(destination);
    }
    
    private PdfString ComputePerms(Span<byte> encryptionKey)
    {
        return PdfString.CreateAscii("12345");
    }

}

public readonly partial struct V6EncryptionKey
{
    [FromConstructor]private readonly byte[] data;
    partial void OnConstructed() => Debug.Assert(data.Length == 48);

    public Span<byte> Hash => data.AsSpan(0, 32);
    public Span<byte> ValidationSalt => data.AsSpan(32,8);
    public Span<byte> KeySalt => data.AsSpan(40,8);
    public Span<byte> WholeKey => data.AsSpan();

    public void SetRandomBits(IRandomNumberSource source) =>
        source.Fill(data.AsSpan(32, 16));

    public static V6EncryptionKey FromRandomSource(IRandomNumberSource source)
    {
        var rawBits = new byte[48];
        var ret = new V6EncryptionKey(rawBits);
        ret.SetRandomBits(source);
        return ret;
    }
    public PdfString AsPdfString() => new(data);
    
}