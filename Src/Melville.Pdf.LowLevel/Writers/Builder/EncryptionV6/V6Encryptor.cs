using System;
using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

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
        var userKey = ComputeUserKey();
        
        return new DictionaryBuilder()
            .WithItem(KnownNames.Filter, KnownNames.Standard)
            .WithItem(KnownNames.V, 5)
            .WithItem(KnownNames.R, 6)
            .WithItem(KnownNames.Length, 256)
            .WithItem(KnownNames.U, userKey)
            .WithItem(KnownNames.O, ComputeOwnerKey(userKey))
            .AsDictionary();
    }


    private PdfString ComputeUserKey() => ComputeGenericKey(UserPassword, Span<byte>.Empty);

    private PdfObject ComputeOwnerKey(PdfString userKey) => ComputeGenericKey(ownerPassword, userKey.Bytes);
    private PdfString ComputeGenericKey(string password, Span<byte> extraBytes)
    {
        var key = V6EncryptionKey.FromRandomSource(random);
        HashAlgorithm2B.ComputePasswordHash(password, key.ValidationSalt, extraBytes, key.Hash, crypto);
        return key.AsPdfString();
        
    }
}

public readonly partial struct V6EncryptionKey
{
    [FromConstructor]private readonly byte[] data;
    partial void OnConstructed() => Debug.Assert(data.Length == 48);

    public Span<byte> Hash => data.AsSpan(0, 32);
    public Span<byte> ValidationSalt => data.AsSpan(32,8);
    public Span<byte> KeySalt => data.AsSpan(40,8);

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