using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.Model.Documents;

internal class ContentStreamConcatReader(Memory<IPdfStreamReader> readers) : IPdfStreamReader
{
    int currentIndex = 0;
    public void Dispose()
    {
        for (int i = currentIndex; i < readers.Length; i++)
        {
            readers.Span[i].Dispose();
        }
    }
    public int Read(Span<byte> buffer)
    {
        while (currentIndex < readers.Length)
        {
            var len = readers.Span[currentIndex].Read(buffer);
            return HandleReadResult(buffer, len);
        }
        return 0;
    }
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellation)
    {
        while (currentIndex < readers.Length)
        {
            var len = await readers.Span[currentIndex].ReadAsync(buffer, cancellation).CA();
            return HandleReadResult(buffer.Span, len);
        }
        return 0;
    }

    private int HandleReadResult(Span<byte> buffer, int len)
    {
        if (len > 0) return len;
        readers.Span[currentIndex].Dispose();
        currentIndex++;
        if (currentIndex >= readers.Length) return 0;
        buffer[0] = (byte)'\n';
        return 1;
    }

    public static async ValueTask<IPdfStreamReader> CreateAsync(PdfArray array)
    {
        var readers = new IPdfStreamReader[array.Count];
        int index = 0;
        await foreach (var directObj in array)
        {
            if (directObj.TryGet<PdfStream>(out var stream))
                readers[index++] = await stream.ReaderForContentAsync().CA();
        }
        return new ContentStreamConcatReader(readers.AsMemory(0, index));
    }
}
