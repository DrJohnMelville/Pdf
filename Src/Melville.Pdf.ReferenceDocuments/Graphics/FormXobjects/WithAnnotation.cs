using Melville.Pdf.LowLevel.Writers.ContentStreams;

namespace Melville.Pdf.ReferenceDocuments.Graphics.FormXobjects
{
    public class WithAnnotation() : Card3x5("The Blue line is in an annotation")
    {
        private PdfIndirectObject form;
        /// <inheritdoc />
        protected override async ValueTask AddContentToDocumentAsync(PdfDocumentCreator docCreator)
        {
            form = docCreator.LowLevelCreator.Add(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.XObject)
                .WithItem(KnownNames.Subtype, KnownNames.Form)
                .WithItem(KnownNames.BBox, new PdfArray(0,0, 200, 200))
                .AsStream("""
                    0 0 1 RG
                    10 w
                    100 0 m
                    100 1000 l
                    S
                    """));
            await base.AddContentToDocumentAsync(docCreator);
        }

        /// <inheritdoc />
        protected override async ValueTask SetPagePropertiesAsync(PageCreator page)
        {
            var ap = new DictionaryBuilder()
                .WithItem(KnownNames.N, form)
                .AsDictionary();
            var annot = new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Annot)
                .WithItem(KnownNames.Subtype, KnownNames.Watermark)
                .WithItem(KnownNames.Rect, new PdfArray(0,0, 300, 300))
                .WithItem(KnownNames.AP, ap)
                .AsDictionary();
            page.AddMetadata(KnownNames.Annots, new PdfArray(annot));
            await base.SetPagePropertiesAsync(page);
        }

        /// <inheritdoc />
        protected override async ValueTask DoPaintingAsync(ContentStreamWriter csw)
        {
            await csw.SetStrokeRGBAsync(1.0, 0, 0);
            csw.SetLineWidth(10);
            csw.MoveTo(0, 100);
            csw.LineTo(1000, 100);
            csw.StrokePath();
        }
    }
}