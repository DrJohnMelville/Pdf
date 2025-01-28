using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

internal readonly partial struct Type3FontFactory
{
    [FromConstructor]private readonly PdfDictionary font;

    public async ValueTask<IRealizedFont> ParseAsync()
    {
        var firstChar = await font.GetOrDefaultAsync(KnownNames.FirstChar, 0).CA();
        var lastChar = await font.GetOrDefaultAsync(KnownNames.LastChar, 255).CA();
        var characters = new PdfStream[1 + lastChar - firstChar];
        var encoding = await font.GetAsync<PdfDictionary>(KnownNames.Encoding).CA();
        var charProcs = await font.GetAsync<PdfDictionary>(KnownNames.CharProcs).CA();
        var differences = await encoding.GetAsync<PdfArray>(KnownNames.Differences).CA();
        int currentChar = 0;
        await foreach (var item in differences.CA())
        {
            switch (item)
            {
                case var x when x.TryGet(out int nextChar): 
                    currentChar = nextChar;
                    break;
                case {IsName:true}:
                    characters[currentChar - firstChar] = await charProcs.GetAsync<PdfStream>(item).CA();
                    currentChar++;
                    break;
            }
        }

        return new RealizedType3Font(characters, (byte)firstChar, 
            await ReadTransformMatrixAsync().CA(), font,
            await ReadWidthsAsync().CA());
    }

    private async ValueTask<IReadOnlyList<double>?> ReadWidthsAsync()
    {
        var widths = await font.GetOrDefaultAsync(KnownNames.Widths, PdfArray.Empty).CA();
        return widths.Count is 0 ? null : await widths.CastAsync<double>().CA();
    }

    private async Task<Matrix3x2> ReadTransformMatrixAsync()
    {
        var digits = await (await font.GetAsync<PdfArray>(KnownNames.FontMatrix).CA()).CastAsync<double>().CA();
        return new Matrix3x2(
            (float) digits[0], (float) digits[1], (float) digits[2], (float) digits[3], (float) digits[4],
            (float) digits[5]);
    }
}