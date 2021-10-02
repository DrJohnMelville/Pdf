using System;
using System.IO;
using Melville.Pdf.LowLevel.Encryption.Cryptography;

namespace Melville.Pdf.LowLevel.Encryption.New
{
    public class Rc4Stream : Stream
    {
        private readonly Stream innerStream;
        private readonly RC4 encryptor;

        public Rc4Stream(Stream innerStream, RC4 encryptor)
        {
            this.innerStream = innerStream;
            this.encryptor = encryptor;
        }

        public override void Flush()
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            innerStream.Dispose();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readSize = innerStream.Read(buffer, offset, count);
            encryptor.TransfromInPlace(buffer.AsSpan(offset, readSize));
            return readSize;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            encryptor.TransfromInPlace(buffer.AsSpan(offset, count));
            innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead => innerStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => innerStream.CanWrite;

        public override long Length => innerStream.Length;

        public override long Position
        {
            get => innerStream.Position;
            set => throw new NotSupportedException();
        }
    }
}