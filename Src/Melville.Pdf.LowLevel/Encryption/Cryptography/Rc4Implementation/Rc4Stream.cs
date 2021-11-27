using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;

public class Rc4Stream : DefaultBaseStream
{
    private readonly Stream innerStream;
    private readonly RC4 encryptor;

    public Rc4Stream(Stream innerStream, RC4 encryptor):
        base(innerStream.CanRead, innerStream.CanWrite, false)
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
    
    public override int Read(Span<byte> buffer)
    {
        var ret = innerStream.Read(buffer);
        encryptor.TransfromInPlace(buffer[..ret]);
        return ret;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        var ret = await innerStream.ReadAsync(buffer, cancellationToken);
        encryptor.TransfromInPlace(buffer.Span[..ret]);
        return ret;
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