using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

internal readonly partial struct DctDecodeParameters
{
    private readonly PdfValueDictionary dict;

    public DctDecodeParameters(PdfDirectValue dict) => this.dict = dict.TryGet(out PdfValueDictionary? temp) ? temp: PdfValueDictionary.Empty;

    public ValueTask<long> ColorTransformAsync() => dict.GetOrDefaultAsync(KnownNames.ColorTransformTName, -1L);
}