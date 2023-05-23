using System;
using System.IO;
using System.Threading.Tasks;
using Melville.JBig2;
using Melville.JBig2.BinaryBitmaps;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter;

internal class JbigToPdfAdapter: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfObject? parameters) => 
        throw new NotSupportedException();

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfObject parameters)
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

        // in the JBIG spec 0 represents a white (background) pixel and 1 represents black,  Since this is the opposite of
        // how DeviceGray color palate works -- we will invert all the bits
        return new InvertingMemoryStream(ary, page.BufferLength());
    }
}