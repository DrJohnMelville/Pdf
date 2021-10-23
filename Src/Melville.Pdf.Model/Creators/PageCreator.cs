using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public class PageCreator: IPageTreeNodeChild
{
    private readonly Dictionary<PdfName, PdfObject> dictionary = new();
    public PageCreator()
    {
        dictionary.Add(KnownNames.Type, KnownNames.Page);
    }

    public (PdfIndirectReference Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize)
    {
        if (parent is null) throw new ArgumentException("Pages must have a parent.");
        dictionary.Add(KnownNames.Parent, parent);
        return (creator.Add(new PdfDictionary(dictionary)), 1);
    }
}