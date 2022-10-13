using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

public readonly partial struct DctDecodeParameters
{
    private readonly PdfDictionary dict;

    public DctDecodeParameters(PdfObject dict) => this.dict = dict as PdfDictionary ?? PdfDictionary.Empty;

    public ValueTask<long> ColorTransformAsync() => dict.GetOrDefaultAsync(KnownNames.ColorTransform, -1);
}