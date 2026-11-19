using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.IStreamAdaptors;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

internal class AesDecodeReader : IPdfStreamReader
{
    private readonly Aes decryptor;
    private readonly ParsingBuffer source;
    private Memory<byte> predecodedBytes = Memory<byte>.Empty;
    private int blockSize;

    public AesDecodeReader(IPdfStreamReader innerReader, Aes decryptor)
    {
        source = new ParsingBuffer(innerReader).WithDisposedDependency(innerReader);
        this.decryptor = decryptor;
        blockSize = decryptor.BlockSize / 8;
    }

    public void Dispose()
    {
        decryptor.Dispose();
        source.Dispose();
    }

    public int Read(Span<byte> buffer)
    {
        using var outputBuffer = new PooledBuffer<byte>(buffer.Length);
        var outputRoot = outputBuffer.AsMemory(0, buffer.Length);
        var nextByte = outputRoot;

        if (TryWritePredecodedBytes(ref nextByte)) return buffer.Length - nextByte.Length;

        while (TryReadBytes(2*blockSize) && nextByte.Length >= blockSize)
        {
            var length = DecodeBlock(nextByte.Span);
            AdvanceBy(ref nextByte, length);
        }
        TryDecodePartialBlock(ref nextByte);
        int bytesWritten = buffer.Length - nextByte.Length;

        outputRoot.Span[..bytesWritten].CopyTo(buffer); 

        return bytesWritten;
    }

    private bool TryReadBytes(int size)
    {
        //ask for one byte more than we need.  This means that either we will have a byte
        //after the current one, and we know we are not at the end, or the DoneReadingSource
        //value will let you know that no more values are available.
        source.DangerousEnsureBytesSync(size + 1);
        return source.CurrentLength >= size;
    }

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        var nextByte = buffer;
        if (TryWritePredecodedBytes(ref nextByte)) return buffer.Length - nextByte.Length;

        // Note that TryReadBytesAsync comes first, we have to try to read on every time
        // TryDecodePartialBlock depends on the prior read attempt.
        while((await TryReadBytesAsync(blockSize*2).CA()) && nextByte.Length >= blockSize)
        {
            var length = DecodeBlock(nextByte.Span);
            AdvanceBy(ref nextByte, length);
        }
        TryDecodePartialBlock(ref nextByte);
        return buffer.Length - nextByte.Length;
    }

    public async ValueTask<bool> TryReadBytesAsync(int size)
    {
        //ask for one byte more than we need.  This means that either we will have a byte
        //after the current one, and we know we are not at the end, or the DoneReadingSource
        //value will let you know that no more values are available.
        await source.TryEnsureBytesAsync(size + 1);
        return source.CurrentLength >= size;
    }

    private void TryDecodePartialBlock(ref Memory<byte> nextByte)
    {
        if (nextByte.IsEmpty || source.CurrentLength < (2 * blockSize)) return;
        predecodedBytes = source.PeekMemory(blockSize);
        Span<byte> target = stackalloc byte[blockSize];
        var length = DecodeBlock(target);
        target.CopyTo(predecodedBytes.Span);
        predecodedBytes = predecodedBytes[..length];
        TryWritePredecodedBytes(ref nextByte);
    }

    bool TryWritePredecodedBytes(ref Memory<byte> target)
    {
        var source = predecodedBytes.Span.TakeMax(target.Length, out var length);
        if (length is 0) return false;
        source.CopyTo(target.Span);
        AdvanceBy(ref target, length);
        AdvanceBy(ref predecodedBytes, length);
        return !predecodedBytes.IsEmpty;
    }
    int DecodeBlock(Span<byte> span)
    {
        var sourceSpan = source.Peek();
        var length = decryptor.DecryptCbc(sourceSpan[blockSize..(2*blockSize)],
            sourceSpan[..blockSize], span, PickPaddinbgMode());
        source.Advance(blockSize);
        return length;
    }
    PaddingMode PickPaddinbgMode() =>
        source.DoneReadingSource && source.CurrentLength == (2 * blockSize) ?
           PaddingMode.PKCS7 : PaddingMode.None;

    void AdvanceBy(ref Memory<byte> target, int length) => target = target[length..];

    private int CopyPreDecodedBytes(ref Memory<byte> nextByte)
    {
        var localSource = predecodedBytes.Span.TakeMax(nextByte.Length, out var len);
        localSource.CopyTo(nextByte.Span);
        nextByte = nextByte[len..];
        predecodedBytes = predecodedBytes[len..];
        return len;
    }
}
