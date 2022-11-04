using System;
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Melville.Pdf.LowLevel.Writers.Builder;

// iText implementation 
// https://github.com/itext/itext7-dotnet/blob/a3ce063817293592763c212248213bbdaa0fc53c/itext/itext.kernel/itext/kernel/crypto/securityhandler/StandardHandlerUsingAes256.cs

public ref struct HashAlgorithm2B
{
    private readonly byte[] kSource;
    private Span<byte> k = Span<byte>.Empty;
    private readonly byte[] k1Source  = Array.Empty<byte>();
    private Span<byte> k1 = Span<byte>.Empty;
    private readonly byte[] encryptedSource = Array.Empty<byte>();
    private Span<byte> encrypted = Span<byte>.Empty;

    private readonly ReadOnlySpan<byte> password;
    private readonly ReadOnlySpan<byte> passwordSalt;
    private readonly ReadOnlySpan<byte> userKeyHash;
    
    private HashAlgorithm[] hashAlgorithms = {
        SHA256.Create(), SHA384.Create(), SHA512.Create(),
    };

    private readonly Aes aes = Aes.Create();
    

    public HashAlgorithm2B(
        ReadOnlySpan<byte> password, ReadOnlySpan<byte> passwordSalt, ReadOnlySpan<byte> userKeyHash)
    {
        this.userKeyHash = userKeyHash;
        this.password = password;
        this.passwordSalt = passwordSalt;
        kSource = ArrayPool<byte>.Shared.Rent(64);
        
        k1Source = ArrayPool<byte>.Shared.Rent(MaxK1Size());
        encryptedSource = ArrayPool<byte>.Shared.Rent(MaxCipherLength());
    }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(kSource);
        ArrayPool<byte>.Shared.Return(k1Source);
        ArrayPool<byte>.Shared.Return(encryptedSource);
    }

    private int MaxCipherLength() => aes.GetCiphertextLengthCbc(MaxK1Size(), PaddingMode.None);
    private int MaxK1Size() => 64 * MaxK1RepeatLength();
    private int MaxK1RepeatLength() => 64 + password.Length + userKeyHash.Length;

    public void ComputeHash(Span<byte> outputBuffer)
    {
        Debug.Assert(outputBuffer.Length >= 32);
        ComputeInitialK();
        DoManyEncryptionRounds();
        k[..32].CopyTo(outputBuffer);
    }

    private void ComputeInitialK()
    {
        Span<byte> concatenated = stackalloc byte[password.Length + passwordSalt.Length];
        password.CopyTo(concatenated);
        passwordSalt.CopyTo(concatenated[password.Length..]);
        ComputeNewK(concatenated, 0);
    }

    private void ComputeNewK(scoped ReadOnlySpan<byte> source, int desiredHash)
    {
        hashAlgorithms[desiredHash].TryComputeHash(source, kSource, out var newKLength);
        k = kSource.AsSpan(..newKLength);
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
        var pos =  WriteFirstK1Segment();
        Make63Copies(ref pos);
        k1 = k1Source.AsSpan(..pos);
    }

    private int WriteFirstK1Segment()
    {
        var pos = 0;
        AppendAt(password, ref pos);
        AppendAt(k, ref pos);
        AppendAt(userKeyHash, ref pos);
        return pos;
    }

    private void Make63Copies(scoped ref int pos)
    {
        var source = new ReadOnlySpan<byte>(k1Source, 0, pos);
        for (int i = 0; i < 63; i++)
        {
            AppendAt(source, ref pos);
        }
    }

    private void AppendAt(scoped in ReadOnlySpan<byte> source, scoped ref int destPos)
    {
        source.CopyTo(k1Source.AsSpan(destPos..));
        destPos += source.Length;
    }
    
    private void DoEncryption()
    {
        aes.Key = MakeArray16(k[..16]);
        var encryptedLength = aes.EncryptCbc(k1, k[16..32], encryptedSource, PaddingMode.None);
        encrypted = encryptedSource.AsSpan(..encryptedLength);
    }
    
    private readonly byte[] byteBuffer = new byte[16];

    private byte[] MakeArray16(in Span<byte> input)
    {
        Debug.Assert(input.Length == 16);
        input.CopyTo(byteBuffer);
        return byteBuffer;
    }


}