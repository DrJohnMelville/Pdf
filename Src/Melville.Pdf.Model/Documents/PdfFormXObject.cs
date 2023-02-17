
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.Model.Documents;

internal class PdfFormXObject: IHasPageAttributes
{
    private readonly PdfStream lowLevel;
    private readonly IHasPageAttributes parent;

    public PdfFormXObject(PdfStream lowLevel, IHasPageAttributes parent)
    {
        this.lowLevel = lowLevel;
        this.parent = parent;
    }

    public PdfDictionary LowLevel => lowLevel;

    public ValueTask<Stream> GetContentBytes() => lowLevel.StreamContentAsync();

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new(parent);

    public async ValueTask<Matrix3x2> Matrix() =>
        await (await lowLevel.GetOrDefaultAsync(KnownNames.Matrix, (PdfArray?)null).CA())
        .AsMatrix3x2OrIdentityAsync().CA();

    public async ValueTask<PdfRect?> Bbox() =>
        (await lowLevel.GetOrDefaultAsync(KnownNames.BBox, PdfArray.Empty).CA())
        is { Count: 4 } arr
            ? await PdfRect.CreateAsync(arr).CA()
            : null;
}