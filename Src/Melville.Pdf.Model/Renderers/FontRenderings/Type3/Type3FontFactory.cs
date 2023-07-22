using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

internal readonly partial struct Type3FontFactory
{
    [FromConstructor]private readonly PdfValueDictionary font;

    public async ValueTask<IRealizedFont> ParseAsync()
    {
        var firstChar = await font.GetOrDefaultAsync(KnownNames.FirstCharTName, 0).CA();
        var lastChar = await font.GetOrDefaultAsync(KnownNames.LastCharTName, 255).CA();
        var characters = new MultiBufferStream[1 + lastChar - firstChar];
        var encoding = await font.GetAsync<PdfDictionary>(KnownNames.EncodingTName).CA();
        var charProcs = await font.GetAsync<PdfDictionary>(KnownNames.CharProcsTName).CA();
        var differences = await encoding.GetAsync<PdfArray>(KnownNames.Differences).CA();
        int currentChar = 0;
        await foreach (var item in differences.CA())
        {
            switch (item)
            {
                case PdfNumber num: 
                    currentChar = (int)num.IntValue;
                    break;
                case PdfName name:
                    var stream = new MultiBufferStream();
                    var source = await (await charProcs.GetAsync<PdfStream>(name).CA()).StreamContentAsync().CA();
                    await source.CopyToAsync(stream).CA();
                    characters[currentChar - firstChar] = stream;
                    currentChar++;
                    break;
            }
        }

        return new RealizedType3Font(characters, (byte)firstChar, await ReadTransformMatrixAsync().CA(), font);
    }

    private async Task<Matrix3x2> ReadTransformMatrixAsync()
    {
        var digits = await (await font.GetAsync<PdfValueArray>(KnownNames.FontMatrixTName).CA()).CastAsync<double>().CA();
        return new Matrix3x2(
            (float) digits[0], (float) digits[1], (float) digits[2], (float) digits[3], (float) digits[4],
            (float) digits[5]);
    }
}