using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.IStreamAdaptors;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Encryption.Cryptography.Rc4Implementation;

internal class Rc4Reader (IPdfStreamReader inner, RC4 encryptor) : IPdfStreamReader
{
    public void Dispose()
    {
        inner.Dispose();
    }

    public int Read(Span<byte> buffer)
    {
        var bytes = inner.Read(buffer);
        encryptor.TransfromInPlace(buffer[..bytes]);
        return bytes;
    }

    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation = default)
    {
        var bytes = await inner.ReadAsync(buffer, cancellation).CA();
        encryptor.TransfromInPlace(buffer.Span[..bytes]);
        return bytes;
    }
}