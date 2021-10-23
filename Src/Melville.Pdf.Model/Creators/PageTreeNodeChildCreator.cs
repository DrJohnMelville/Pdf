using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public abstract class PageTreeNodeChildCreator
{
    protected Dictionary<PdfName, PdfObject> MetaData { get; }
    protected Dictionary<PdfName, PdfObject> Resources { get; } = new();

    protected PageTreeNodeChildCreator(Dictionary<PdfName, PdfObject> metaData)
    {
        MetaData = metaData;
    }

    public abstract (PdfIndirectReference Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize);

    protected void TryAddResources(ILowLevelDocumentCreator creator)
    {
        if (Resources.Count > 0) 
            MetaData.Add(KnownNames.Resources, creator.Add(new PdfDictionary(Resources)));
    }
}