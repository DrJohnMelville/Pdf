using System;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using Melville.Pdf.LowLevel.Encryption.StringFilters;

namespace Melville.Pdf.LowLevel.Writers.Builder.EncryptionV6;

// iText implementation 
// https://github.com/itext/itext7-dotnet/blob/a3ce063817293592763c212248213bbdaa0fc53c/itext/itext.kernel/itext/kernel/crypto/securityhandler/StandardHandlerUsingAes256.cs

internal ref struct HashAlgorithm2B
{
    public static void ComputePasswordHash(
        string passwordText, Span<byte> salt, Span<byte> extraBytes, Span<byte> target, 
        V6Cryptography crypto)
    {
        Span<byte> paddedPassword = stackalloc byte[127];
        Encoding.UTF8.GetEncoder().Convert(
            passwordText.SaslPrep(), paddedPassword, true, out _, out var bytesUsed, out _);
        using var hasher = new HashAlgorithm2B(paddedPassword[..bytesUsed], salt,
            extraBytes, crypto);
        hasher.ComputeHash(target);
    }

    
    private readonly byte[] kSource;
    private Span<byte> k = Span<byte>.Empty;
    private readonly byte[] k1Source  = Array.Empty<byte>();
    private Span<byte> k1 = Span<byte>.Empty;
    private readonly byte[] encryptedSource = Array.Empty<byte>();
    private Span<byte> encrypted = Span<byte>.Empty;

    private readonly ReadOnlySpan<byte> password;
    private readonly ReadOnlySpan<byte> passwordSalt;
    private readonly ReadOnlySpan<byte> userKeyHash;

    private readonly V6Cryptography crypto;

    private HashAlgorithm2B(
        ReadOnlySpan<byte> password, ReadOnlySpan<byte> passwordSalt, ReadOnlySpan<byte> userKeyHash,
        V6Cryptography crypto)
    {
        this.userKeyHash = userKeyHash;
        this.crypto = crypto;
        this.password = password;
        this.passwordSalt = passwordSalt;
        kSource = ArrayPool<byte>.Shared.Rent(64);
        
        k1Source = ArrayPool<byte>.Shared.Rent(MaxK1Size());
        encryptedSource = ArrayPool<byte>.Shared.Rent(MaxCipherLength());
    }

    private void Dispose()
    {
        ArrayPool<byte>.Shared.Return(kSource);
        ArrayPool<byte>.Shared.Return(k1Source);
        ArrayPool<byte>.Shared.Return(encryptedSource);
    }

    private int MaxCipherLength() => crypto.Cbc.CipherLength(MaxK1Size());
    private int MaxK1Size() => 64 * MaxK1RepeatLength();
    private int MaxK1RepeatLength() => 64 + password.Length + userKeyHash.Length;

    private void ComputeHash(Span<byte> outputBuffer)
    {
        Debug.Assert(outputBuffer.Length >= 32);
        ComputeInitialK();
        DoManyEncryptionRounds();
        k[..32].CopyTo(outputBuffer);
    }

    private void ComputeInitialK()
    {
        Span<byte> concatenated = stackalloc byte[password.Length + passwordSalt.Length + userKeyHash.Length];
        password.CopyTo(concatenated);
        passwordSalt.CopyTo(concatenated[password.Length..]);
        userKeyHash.CopyTo(concatenated[(password.Length+passwordSalt.Length)..]);
        ComputeNewK(concatenated, 0);
    }

    private void ComputeNewK(scoped Span<byte> source, int desiredHash)
    {
        k = crypto.Hash(desiredHash, source, kSource);
    }
    
    private static readonly char[] hexDigits = { '0', '1', '2', '3','4', '5','6','7','8','9','A','B','C','D','E','F'};
    private static string HexFromBits(ReadOnlySpan<byte> bits)
    {
        Span<char> ret = stackalloc char[bits.Length * 2];
        int position = 0;
        foreach (var item in bits)
        {
            ret[position++] = hexDigits[item >> 4];
            ret[position++] = hexDigits[item & 0xF];
        }

        return new string(ret);
    }

    private void DoManyEncryptionRounds()
    {
        int round;
        for (round = 0; round < 64; round++)
        {
            DoSingleRound();
        }
        for (; !IsProperEndingRound(round); round++)
        {
            DoSingleRound();
        }
    }

    private bool IsProperEndingRound(int round) => encrypted[^1] <= (round - 32);

    private void DoSingleRound()
    {
        ComputeK1();
        DoEncryption();
        ComputeNewK(encrypted, encrypted[..16].Mod3());
    }
    
    private void ComputeK1()
    {
        var writer = new SpanWriter(k1Source);
        WriteFirstK1Segment(ref writer);
        writer.DuplicateNTimes(63);
        k1 = writer.BuiltSpan();
    }

    private void WriteFirstK1Segment(ref SpanWriter writer)
    {
        writer.Append(password);
        writer.Append(k);
        writer.Append(userKeyHash);
    }

    private void DoEncryption()
    {
        encrypted = crypto.Cbc.Encrypt(k[..16], k[16..32], k1, encryptedSource);
    }
}