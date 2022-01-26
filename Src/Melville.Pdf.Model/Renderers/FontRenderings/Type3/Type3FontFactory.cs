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
        var firstChar = await font.GetOrDefaultAsync(KnownNames.FirstChar, 0).ConfigureAwait(false);
        var lastChar = await font.GetOrDefaultAsync(KnownNames.LastChar, 255).ConfigureAwait(false);
        var characters = new MultiBufferStream[1 + lastChar - firstChar];
        var encoding = await font.GetAsync<PdfDictionary>(KnownNames.Encoding).ConfigureAwait(false);
        var charProcs = await font.GetAsync<PdfDictionary>(KnownNames.CharProcs).ConfigureAwait(false);
        var differences = await encoding.GetAsync<PdfArray>(KnownNames.Differences).ConfigureAwait(false);
        int currentChar = 0;
        await foreach (var item in differences.ConfigureAwait(false))
        {
            switch (item)
            {
                case PdfNumber num: 
                    currentChar = (int)num.IntValue;
                    break;
                case PdfName name:
                    var stream = new MultiBufferStream();
                    var source = await (await charProcs.GetAsync<PdfStream>(name).ConfigureAwait(false)).StreamContentAsync().ConfigureAwait(false);
                    await source.CopyToAsync(stream).ConfigureAwait(false);
                    characters[currentChar - firstChar] = stream;
                    currentChar++;
                    break;
            }
        }

        return new NamedDefaultMapping(new RealizedType3Font(target, characters, (byte)firstChar,
            (await ReadTransformMatrix().ConfigureAwait(false)*
             Matrix3x2.CreateScale((float)size, (float)size))), false, false,
            NullUnicodeMapping.Instance);
    }

    private async Task<Matrix3x2> ReadTransformMatrix()
    {
        var digits = await (await font.GetAsync<PdfArray>(KnownNames.FontMatrix).ConfigureAwait(false)).AsDoublesAsync().ConfigureAwait(false);
        return new Matrix3x2(
            (float) digits[0], (float) digits[1], (float) digits[2], (float) digits[3], (float) digits[4],
            (float) digits[5]);
    }
}