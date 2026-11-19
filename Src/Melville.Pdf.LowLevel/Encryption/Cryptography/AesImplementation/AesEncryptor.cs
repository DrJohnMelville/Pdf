using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

internal class AesEncryptor : ICipherOperations
{
    private readonly Aes encryptor;

    public AesEncryptor(Aes encryptor)
    {
        this.encryptor = encryptor;
    }

    public Span<byte> CryptSpan(Span<byte> input)
    {
        var blockLength = encryptor.BlockSize / 8;

        var final = new byte[input.Length + (2 * blockLength)]; // IV plus a possible termination block.
        Span<byte> iv = final.AsSpan(0, blockLength);
        RandomNumberGenerator.Fill(iv);

        var length = encryptor.EncryptCbc(input, iv, final.AsSpan(blockLength));
        return final.AsSpan(0, blockLength + length);
    }

    public IPdfStreamReader CryptStream(IPdfStreamReader input) =>
        new AesEncryptingReader(input, encryptor);
}

internal class AesEncryptingReader : IPdfStreamReader
{
    private readonly ParsingBuffer source;
    private readonly byte[] outputBuffer;
    private readonly Aes encoder;
    private Memory<byte> nextBytes;
    private readonly int blockBytes;
    private bool hasWrittenTrailer;

    public AesEncryptingReader(IPdfStreamReader sourceReader, Aes encoder)
    {
        this.encoder = encoder;
        source = new ParsingBuffer(sourceReader).WithDisposedDependency(sourceReader);
        blockBytes = encoder.BlockSize / 8;
        outputBuffer = new byte[blockBytes*2];
        nextBytes = outputBuffer.AsMemory(0, blockBytes);
        RandomNumberGenerator.Fill(nextBytes.Span);
    }

    public void Dispose() => source.Dispose();

    public int Read(Span<byte> buffer)
    {
        var target = buffer;
        while (!target.IsEmpty)
        {
            TryWriteNextBytes(ref target);
            if (nextBytes.IsEmpty)
            {
                if (!source.DangerousEnsureBytesSync(blockBytes)) break;
            }
        }
        return buffer.Length - target.Length;
    }

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        var target = buffer;
        while (!target.IsEmpty)
        {
            TryWriteNextBytes(ref target);
            if (nextBytes.IsEmpty)
            {
                if (!EncodingLoop(await source.TryEnsureBytesAsync(blockBytes).CA())) break;
            }
        }
        return buffer.Length - target.Length;
    }

    private bool EncodingLoop(bool hasBytes)
    {
        if (hasBytes)
        {
            DoEncryptionRound(PaddingMode.None);
        }
        else
        {
            if (hasWrittenTrailer) return false;
            DoEncryptionRound(PaddingMode.PKCS7);
            hasWrittenTrailer = true;
        }

        return true;
    }

    private void TryWriteNextBytes(ref Memory<byte> target)
    {
        var nextSpan = nextBytes.Span.TakeMax(target.Length, out var length);
        if (length > 0)
        {
            nextSpan.CopyTo(target.Span);
            target = target[length..];
            nextBytes = nextBytes[length..];
        }
    }
    private void TryWriteNextBytes(ref Span<byte> target)
    {
        var nextSpan = nextBytes.Span.TakeMax(target.Length, out var length);
        if (length > 0)
        {
            nextSpan.CopyTo(target);
            target = target[length..];
            nextBytes = nextBytes[length..];
        }
    }

    private void DoEncryptionRound(PaddingMode paddingMode)
    {
        Debug.Assert(nextBytes.IsEmpty);
        int written = encoder.EncryptCbc(
            source.Peek(blockBytes), outputBuffer.AsSpan(0,blockBytes), outputBuffer, paddingMode);
        nextBytes = outputBuffer.AsMemory(0, written);
        source.Advance(Math.Min(blockBytes, source.CurrentLength));
    }
}