using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation;

internal class AesDecodeStream : DefaultBaseStream
        
{
    private readonly Stream input;
    private readonly Aes decryptor;
    private CryptoStream? decryptedSource;

    public AesDecodeStream(Stream input, Aes decryptor): base(true, false, false)
    {
        this.input = input;
        this.decryptor = decryptor;
    }

    protected override void Dispose(bool disposing)
    {
        input.Dispose();
        base.Dispose(true);
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        if (decryptedSource == null) await InitalizeCryptStream().CA();
        return await decryptedSource!.ReadAsync(buffer, cancellationToken).CA();
    }

    private async ValueTask InitalizeCryptStream()
    {
        var iv = new byte[decryptor.BlockSize / 8];
        await iv.FillBufferAsync(0, iv.Length, input).CA();
        decryptor.IV = iv;
        decryptedSource = new CryptoStream(input, decryptor.CreateDecryptor(), CryptoStreamMode.Read);
    }
}