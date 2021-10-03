using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.AesImplementation
{
    public class AesDecodeStream : SequentialReadFilterStream
        
    {
        private readonly Stream input;
        private readonly Aes decryptor;
        private CryptoStream? decryptedSource;

        public AesDecodeStream(Stream input, Aes decryptor)
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
            if (decryptedSource == null) await InitalizeCryptStream();
            return await decryptedSource!.ReadAsync(buffer, cancellationToken);
        }

        private async ValueTask InitalizeCryptStream()
        {
            var iv = new byte[decryptor.BlockSize / 8];
            await iv.FillBufferAsync(0, iv.Length, input);
            decryptor.IV = iv;
            decryptedSource = new CryptoStream(input, decryptor.CreateDecryptor(), CryptoStreamMode.Read);
        }
    }
}