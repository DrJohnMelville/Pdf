using System;
using System.Collections.Generic;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Microsoft.VisualBasic;

namespace Melville.Pdf.Model.Creators;

public class PageCreator: PageTreeNodeChildCreator
{
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
        TryAddResources(creator);
        return (creator.Add(new PdfDictionary(MetaData)), 1);
    }
    
    public void AddLastModifiedTime(PdfTime dateAndTime) => 
        MetaData.Add(KnownNames.LastModified, PdfString.CreateDate(dateAndTime));
}