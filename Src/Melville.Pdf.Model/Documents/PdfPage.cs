using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.Model.Documents;

public readonly struct PdfFormXObject: IHasPageAttributes
{
    private readonly PdfStream lowLevel;
    private readonly IHasPageAttributes parent;

    public PdfFormXObject(PdfStream lowLevel, IHasPageAttributes parent) : this()
    {
        this.lowLevel = lowLevel;
        this.parent = parent;
    }

    public PdfDictionary LowLevel => lowLevel;

    public ValueTask<Stream> GetContentBytes() => lowLevel.StreamContentAsync();

    public ValueTask<IHasPageAttributes?> GetParentAsync() => new(parent);
}

public readonly struct PdfPage : IHasPageAttributes
{
    public PdfDictionary LowLevel { get; }

    public PdfPage(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }

    public async ValueTask<PdfTime?> LastModifiedAsync()
    {
        return LowLevel.TryGetValue(KnownNames.LastModified, out var task) &&
               await task is PdfString str
            ? str.AsPdfTime()
            : null;
    }

    public async ValueTask<Stream> GetContentBytes() =>
        await LowLevel.GetOrNullAsync(KnownNames.Contents) switch
        {
            PdfStream strm => await strm.StreamContentAsync(),
            PdfArray array => new PdfArrayConcatStream(array),
            var x => throw new PdfParseException("Could not find content stream")
        };

    public ValueTask<IHasPageAttributes?> GetParentAsync() =>
        PdfPageAttributes.ParentFromAttribute(this);
}