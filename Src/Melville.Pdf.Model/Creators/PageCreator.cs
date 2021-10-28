using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Writers;
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
        builder.DisposeAsync().GetAwaiter().GetResult();
        return ret;
    }
}
public class PageCreator: PageTreeNodeChildCreator
{
    private readonly List<PdfStream> streamSegments = new();
    public PageCreator() : base(new())
    {
        MetaData.Add(KnownNames.Type, KnownNames.Page);
    }

    public override (PdfIndirectReference Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize)
    {
        if (parent is null) throw new ArgumentException("Pages must have a parent.");
        MetaData.Add(KnownNames.Parent, parent);
        TryAddContent(creator);
        TryAddResources(creator);
        return (creator.Add(new PdfDictionary(MetaData)), 1);
    }

    private void TryAddContent(ILowLevelDocumentCreator creator)
    {
        if (streamSegments.Count > 0)
            MetaData.Add(KnownNames.Contents, CreateContents(creator));
    }

    private PdfObject CreateContents(ILowLevelDocumentCreator creator) =>

        streamSegments.Count == 1
            ? CreateStreamSegment(creator, streamSegments[0])
            : new PdfArray(streamSegments.Select(i => CreateStreamSegment(creator, i)));

    private PdfIndirectReference CreateStreamSegment(ILowLevelDocumentCreator creator, PdfStream stream) => 
        creator.Add(stream);

    public void AddLastModifiedTime(PdfTime dateAndTime) => 
        MetaData.Add(KnownNames.LastModified, PdfString.CreateDate(dateAndTime));

    public void AddToContentStream(PdfStream data) => streamSegments.Add(data);
}