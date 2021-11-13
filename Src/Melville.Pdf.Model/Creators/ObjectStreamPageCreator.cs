using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public class ObjectStreamPageCreator : PageCreator
{
    public override (PdfIndirectReference Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator,
            PdfIndirectReference? parent, int maxNodeSize)
    {
        var builder = creator.ObjectStreamContext();
        var ret = base.ConstructPageTree(creator, parent, maxNodeSize);
        builder.Dispose();
        return ret;
    }
}