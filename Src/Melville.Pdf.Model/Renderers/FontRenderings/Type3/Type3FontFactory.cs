using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.FontMappings;

namespace Melville.Pdf.Model.Renderers.FontRenderings.Type3;

public readonly struct Type3FontFactory
{
    private readonly PdfDictionary font;
    private readonly IFontTarget target;
    private readonly double size;

    public Type3FontFactory(PdfDictionary font, IFontTarget target, double size)
    {
        this.font = font;
        this.target = target;
        this.size = size;
    }

    public async ValueTask<IFontMapping> ParseAsync()
    {
        var firstChar = await font.GetOrDefaultAsync(KnownNames.FirstChar, 0);
        var lastChar = await font.GetOrDefaultAsync(KnownNames.LastChar, 255);
        var characters = new MultiBufferStream[1 + lastChar - firstChar];
        var encoding = await font.GetAsync<PdfDictionary>(KnownNames.Encoding);
        var charProcs = await font.GetAsync<PdfDictionary>(KnownNames.CharProcs);
        var differences = await encoding.GetAsync<PdfArray>(KnownNames.Differences);
        int currentChar = 0;
        await foreach (var item in differences)
        {
            switch (item)
            {
                case PdfNumber num: 
                    currentChar = (int)num.IntValue;
                    break;
                case PdfName name:
                    var stream = new MultiBufferStream();
                    var source = await (await charProcs.GetAsync<PdfStream>(name)).StreamContentAsync();
                    await source.CopyToAsync(stream);
                    characters[currentChar - firstChar] = stream;
                    currentChar++;
                    break;
            }
        }

        return new NamedDefaultMapping(new RealizedType3Font(target, characters, (byte)firstChar,
            (await ReadTransformMatrix()*
             Matrix3x2.CreateScale((float)size, (float)size))), false, false,
            NullUnicodeMapping.Instance);
    }

    private async Task<Matrix3x2> ReadTransformMatrix()
    {
        var digits = await (await font.GetAsync<PdfArray>(KnownNames.FontMatrix)).AsDoublesAsync();
        return new Matrix3x2(
            (float) digits[0], (float) digits[1], (float) digits[2], (float) digits[3], (float) digits[4],
            (float) digits[5]);
    }
}