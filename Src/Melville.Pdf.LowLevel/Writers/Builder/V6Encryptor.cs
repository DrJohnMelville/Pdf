using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.StringFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Writers.Builder;

public partial class V6Encryptor: ILowLevelDocumentEncryptor
{
    [FromConstructor]public string UserPassword { get; set; }
    [FromConstructor]private readonly string ownerPassword;
    [FromConstructor]private readonly PdfPermission permissionsRestricted;
    [FromConstructor]private readonly PdfName? defStream;
    [FromConstructor]private readonly PdfName? defString;
    [FromConstructor]private readonly PdfName? defEmbeddedFile;
    [FromConstructor]private readonly V4CfDictionary dictionary;
    [FromConstructor] private readonly IRandomNumberSource random;

    public V6Encryptor(
        string userPassword, string ownerPassword, PdfPermission permissionsRestricted, PdfName? defStream, 
        PdfName? defString, PdfName? defEmbeddedFile, V4CfDictionary dictionary):
        this (userPassword, ownerPassword, permissionsRestricted, defStream, defString, defEmbeddedFile,
            dictionary, RandomNumberSource.Instance)
    {
    }

    public PdfDictionary CreateEncryptionDictionary(PdfArray id)
    {
        return new DictionaryBuilder()
            .WithItem(KnownNames.Filter, KnownNames.Standard)
            .WithItem(KnownNames.V, 5)
            .WithItem(KnownNames.R, 6)
            .WithItem(KnownNames.Length, 256)
            .WithItem(KnownNames.U, ComputeUserKey())
            .AsDictionary();
    }

    private PdfString ComputeUserKey()
    {
        var rawBits = new byte[48];
        var userKey = new V6EncryptionKey(rawBits);
        userKey.SetRandomBits(random);

        Span<byte> paddedPassword = stackalloc byte[135];
        Encoding.UTF8.GetEncoder().Convert(
            UserPassword.SaslPrep(), userKey, true, out _, out var bytesUsed, out _);
        userKey.ValidationSalt.CopyTo(paddedPassword[Math.Min(bytesUsed, 127)..]);
        ComputeHash(paddedPassword.Slice(0, bytesUsed + 8), ReadOnlySpan<byte>.Empty, userKey.Hash);

        return new PdfString(rawBits);
    }
    // iText implementation 
    // https://github.com/itext/itext7-dotnet/blob/a3ce063817293592763c212248213bbdaa0fc53c/itext/itext.kernel/itext/kernel/crypto/securityhandler/StandardHandlerUsingAes256.cs

    private void ComputeHash(
        ReadOnlySpan<byte> valueToHash, ReadOnlySpan<byte> userKeyString, Span<byte> userKeyHash)
    {
        Span<byte> K = stackalloc byte[64];
        var sha256 = SHA256.Create();
        sha256.TryComputeHash(valueToHash, K, out var kLength);
        for (int round = 0; round < 64 && IsDone(K, kLength, round); round++)
        {
            Resume coding here.
        }
    }

    private bool IsDone(Span<byte> K, int Klength, int round) => K[Klength - 1] <= (round - 32);
}

public readonly partial struct V6EncryptionKey
{
    [FromConstructor]private readonly Memory<byte> data;
    partial void OnConstructed() => Debug.Assert(data.Length == 48);

    public Span<byte> Hash => data.Span.Slice(0, 32);
    public Span<byte> ValidationSalt => data.Span.Slice(32,8);
    public Span<byte> KeySalt => data.Span.Slice(40,8);

    public void SetRandomBits(IRandomNumberSource source) =>
        source.Fill(data.Span.Slice(32, 16));
}