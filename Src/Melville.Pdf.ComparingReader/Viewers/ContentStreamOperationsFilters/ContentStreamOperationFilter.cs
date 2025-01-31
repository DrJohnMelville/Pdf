using Melville.INPC;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.ComparingReader.Viewers.ContentStreamOperationsFilters;

internal partial class ContentStreamOperationFilter(
    IContentStreamOperations inner): IContentStreamOperations
{
    [DelegateTo]private IContentStreamOperations target = inner;
    private IContentStreamOperations renderingTarget = inner;
    
    /// <inheritdoc />
    public void MarkedContentPoint(PdfDirectObject tag)
    {
        if (tag.Equals(KnownNames.RenderOff))
        {
            target = NopContentStreamOperations.Instance;
        }
        target.MarkedContentPoint(tag);
        if (tag.Equals(KnownNames.RenderOn))
        {
            target = renderingTarget;
        }
    }

    public static IContentStreamOperations Wrap(IContentStreamOperations inner) => 
        new ContentStreamOperationFilter(inner);
}