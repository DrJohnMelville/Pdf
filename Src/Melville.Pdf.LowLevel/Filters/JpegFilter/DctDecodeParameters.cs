using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

internal readonly partial struct DctDecodeParameters
{
    private readonly PdfDictionary dict;

    public DctDecodeParameters(PdfDirectObject dict) => this.dict = dict.TryGet(out PdfDictionary? temp) ? temp: PdfDictionary.Empty;

    public ValueTask<long> ColorTransformAsync() => dict.GetOrDefaultAsync(KnownNames.ColorTransform, -1L);
}