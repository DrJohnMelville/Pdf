using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.Model.Renderers.Annotations;

internal readonly partial struct Annotation
{
    [FromConstructor] public PdfDictionary LowLevel { get; }

    public async ValueTask<PdfStream?> GetVisibleFormAsync() =>
        await LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.AP).CA() is { } appearance &&
        await appearance.GetOrNullAsync<PdfDictionary>(KnownNames.N).CA() is { } normalAp
            ? await TryPickAppearanceStateAsync(normalAp).CA()
            : null;

    private async Task<PdfStream?> TryPickAppearanceStateAsync(PdfDictionary normalAp) =>
        await LowLevel.GetOrNullAsync(KnownNames.AS).CA() is { IsName: true } state &&
        await normalAp.GetOrNullAsync<PdfStream>(state).CA() is { } stateAp
            ? stateAp : normalAp as PdfStream;
   
    public async ValueTask<PdfRect?> RectAsync() =>
        (await LowLevel.GetOrDefaultAsync(KnownNames.Rect, PdfArray.Empty).CA())
        is { Count: 4 } arr
            ? await PdfRect.CreateAsync(arr).CA()
            : null;

}