using System;
using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

public class JbigToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters) => 
        throw new NotSupportedException();

    public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        var reader = new JbigExplicitPageReader();
        reader.RequestPage(1);
        if (parameters is PdfDictionary dict &&
            (await dict.GetOrDefaultAsync<PdfStream?>(KnownNames.JBIG2Globals, null).CA()) is { } globals)
        {
            await reader.ProcessSequentialSegments(await globals.StreamContentAsync().CA(), 1).CA();
        }

        await reader.ProcessSequentialSegments(input, 1).CA();
        var page = reader.GetPage(1);
        var (ary, _) = page.ColumnLocation(0);
        return new InvertingMemoryStream(ary, page.BufferLength());
    }
}

public sealed partial class InvertingMemoryStream : DefaultBaseStream
{
    private readonly byte[] data;
    private readonly int length;
    private int offset = 0;

    public InvertingMemoryStream(byte[] data, int length): base(true, false, false)
    {
        this.data = data;
        this.length = length;
    }

    private int RemainingBytes => length - offset;

    public override int Read(Span<byte> buffer)
    {
        var len = Math.Min(buffer.Length, RemainingBytes);
        for (int i = 0; i < len; i++)
        {
            buffer[i] = (byte) ~data[offset++];
        }
        return len;
    } 
}