using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.Model.Documents;

namespace Melville.Pdf.Model.Creators;

public abstract class PageTreeNodeChildCreator
{
    protected Dictionary<PdfName, PdfObject> MetaData { get; }
    protected Dictionary<(PdfName DictionaryName, PdfName ItemName), PdfObject> Resources { get; } = new();

    protected PageTreeNodeChildCreator(Dictionary<PdfName, PdfObject> metaData)
    {
        MetaData = metaData;
    }

    public abstract (PdfIndirectReference Reference, int PageCount) 
        ConstructPageTree(ILowLevelDocumentCreator creator, PdfIndirectReference? parent,
            int maxNodeSize);

    protected void TryAddResources(ILowLevelDocumentCreator creator)
    {
        if (Resources.Count == 0) return;
        var res = new Dictionary<PdfName, PdfObject>();
        foreach (var subDictionary in Resources.GroupBy(i=>i.Key.DictionaryName))
        {
            res.Add(subDictionary.Key, DictionaryValues(creator, subDictionary));
        }
        MetaData.Add(KnownNames.Resources, new PdfDictionary(res));
    }

    private static PdfObject DictionaryValues(ILowLevelDocumentCreator creator, IGrouping<PdfName, KeyValuePair<(PdfName DictionaryName, PdfName ItemName), PdfObject>> subDictionary) =>
        subDictionary.Key == KnownNames.ProcSet? 
            subDictionary.First().Value:
            new PdfDictionary(subDictionary
                .Select(i => (i.Key.ItemName, creator.Add(i.Value) as PdfObject)).ToList());

    public void AddXrefObjectResource(PdfName name, PdfObject obj) =>
        Resources[(KnownNames.XObject, name)] = obj;

    public void AddMediaBox(in PdfRect pdfRect) => MetaData.Add(KnownNames.MediaBox, pdfRect.ToPdfArray);
    public void AddCropBox(in PdfRect pdfRect) => MetaData.Add(KnownNames.CropBox, pdfRect.ToPdfArray);
    public void AddBleedBox(in PdfRect pdfRect) => MetaData.Add(KnownNames.BleedBox, pdfRect.ToPdfArray);
    public void AddTrimBox(in PdfRect pdfRect) => MetaData.Add(KnownNames.TrimBox, pdfRect.ToPdfArray);
    public void AddArtBox(in PdfRect pdfRect) => MetaData.Add(KnownNames.ArtBox, pdfRect.ToPdfArray);
}