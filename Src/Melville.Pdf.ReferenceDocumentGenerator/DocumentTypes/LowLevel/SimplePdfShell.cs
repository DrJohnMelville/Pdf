using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel
{
    public class PdfCreator
    {
        public ILowLevelDocumentCreator Creator { get; }
        private List<PdfIndirectReference> pages = new ();
        public PdfIndirectReference PagesParent { get; }

        private readonly LazyPdfObject defaultProcSet;
        public PdfIndirectReference DefaultProcSet => defaultProcSet.Value;

        private readonly LazyPdfObject defaultFont;
        public PdfIndirectReference DefaultFont => defaultFont.Value;

        public PdfCreator(int major = 1, int minor = 7)
        {
            Creator = new LowLevelDocumentCreator();
            Creator.SetVersion((byte)major, (byte)minor);
            PagesParent = Creator.AsIndirectReference();
            defaultProcSet = new LazyPdfObject(Creator, () => new PdfArray(KnownNames.PDF));
            defaultFont = new LazyPdfObject(Creator, () => 
                new PdfDictionary(
                    (KnownNames.Type, KnownNames.Font ), 
                    (KnownNames.Subtype, KnownNames.Type1), 
                    (KnownNames.Name, new PdfName("F1")), 
                    (KnownNames.BaseFont,
                        KnownNames.Helvetica), 
                    (KnownNames.Encoding, KnownNames.MacRomanEncoding))
            );
        }

        public void AddPageToPagesCollection(PdfObject page) => 
            pages.Add(Creator.AsIndirectReference(page));

        public PdfDictionary CreateUnattachedPage(PdfIndirectReference content) => new(
                (KnownNames.Type, KnownNames.Page),
                (KnownNames.Parent, PagesParent), (KnownNames.MediaBox, new PdfArray(
                    new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))),
                (KnownNames.Contents, content),
                (KnownNames.Resources,
                    new PdfDictionary(
                        (KnownNames.Font, new PdfDictionary((new PdfName("F1"), DefaultFont))),
                        (KnownNames.ProcSet, DefaultProcSet))));

        public PdfDictionary CreateUnattachedPage(PdfStream content) =>
            CreateUnattachedPage(Creator.AsIndirectReference(content));

        public async ValueTask<PdfDictionary> CreateUnattachedPageAsync(StreamDataSource source) =>
            CreateUnattachedPage(
                Creator.Add(
                    await Creator.NewCompressedStream(source, KnownNames.FlateDecode)));

        public async ValueTask CreateAttachedPage(StreamDataSource source)
        {
            var page = await CreateUnattachedPageAsync(source);
            AddPageToPagesCollection(Creator.Add(page));
        }

        public void FinalizePages()
        {
            var catalog = Creator.AsIndirectReference();
            var outlines = Creator.AsIndirectReference(new PdfDictionary((KnownNames.Type, KnownNames.Outlines), (KnownNames.Count, new PdfInteger(0))));
            Creator.AssignValueToReference(PagesParent, new PdfDictionary((KnownNames.Type, KnownNames.Pages), (KnownNames.Kids, new PdfArray(pages)), (KnownNames.Count, new PdfInteger(pages.Count))));
            Creator.AssignValueToReference(catalog, new PdfDictionary((KnownNames.Type, KnownNames.Catalog), (KnownNames.Outlines, outlines), (KnownNames.Pages, pagesArray: PagesParent)));

            Creator.Add(catalog);
            Creator.Add(outlines);
            Creator.Add(PagesParent);
            
            Creator.AddToTrailerDictionary(KnownNames.Root, catalog);
            
        }
    }

    public static class SimplePdfShell
    {
        public static async ValueTask<ILowLevelDocumentCreator> Generate(
            int major, int minor, 
            Func<ILowLevelDocumentCreator,PdfIndirectReference, 
                ValueTask<IReadOnlyList<PdfObject>>> createPages)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion((byte)major, (byte)minor);
            var catalog = builder.AsIndirectReference();
            var outlines = builder.AsIndirectReference(new PdfDictionary((KnownNames.Type, KnownNames.Outlines), (KnownNames.Count, new PdfInteger(0))));
            var pages = builder.AsIndirectReference();
            var pageGroup = await createPages(builder, pages);
            builder.AssignValueToReference(pages, new PdfDictionary((KnownNames.Type, KnownNames.Pages), (KnownNames.Kids, new PdfArray(pageGroup)), (KnownNames.Count, new PdfInteger(pageGroup.Count))));
            builder.AssignValueToReference(catalog, new PdfDictionary((KnownNames.Type, KnownNames.Catalog), (KnownNames.Outlines, outlines), (KnownNames.Pages, pages)));

            builder.Add(catalog);
            builder.Add(outlines);
            builder.Add(pages);
            
            builder.AddToTrailerDictionary(KnownNames.Root, catalog);
            return builder;       
        }

    }
}