
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.Model.Documents;

internal partial class PdfFormXObject: IHasPageAttributes
{
    [FromConstructor] private readonly PdfValueStream lowLevel;
    [FromConstructor] private readonly IHasPageAttributes parent;

    public PdfValueDictionary LowLevel => lowLevel;

    public ValueTask<Stream> GetContentBytesAsync() => lowLevel.StreamContentAsync();

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new(parent);

    public async ValueTask<Matrix3x2> MatrixAsync() =>
        await (await lowLevel.GetOrDefaultAsync(KnownNames.MatrixTName, (PdfValueArray?)null).CA())
        .AsMatrix3x2OrIdentityAsync().CA();

    public async ValueTask<PdfRect?> BboxAsync() =>
        (await lowLevel.GetOrDefaultAsync(KnownNames.BBoxTName, PdfValueArray.Empty).CA())
        is { Count: 4 } arr
            ? await PdfRect.CreateAsync(arr).CA()
            : null;
}