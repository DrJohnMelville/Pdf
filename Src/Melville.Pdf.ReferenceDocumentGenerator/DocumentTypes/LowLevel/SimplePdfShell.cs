using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
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
                    (KnownNames.BaseFont, KnownNames.Helvetica), 
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
        
        public PdfDictionary CreateUnattachedPage(StreamDataSource source) =>
            CreateUnattachedPage(
                Creator.Add(source.AsStream()));

        public void CreateAttachedPage(StreamDataSource source)
        {
            var page = CreateUnattachedPage(source);
            AddPageToPagesCollection(Creator.Add(page));
        }

        public void FinalizePages()
        {
            AddDefaultObjects();
            var catalog = Creator.AsIndirectReference();
            var outlines = Creator.AsIndirectReference(new PdfDictionary((KnownNames.Type, KnownNames.Outlines), (KnownNames.Count, new PdfInteger(0))));
            Creator.AssignValueToReference(PagesParent, new PdfDictionary((KnownNames.Type, KnownNames.Pages), (KnownNames.Kids, new PdfArray(pages)), (KnownNames.Count, new PdfInteger(pages.Count))));
            Creator.AssignValueToReference(catalog, new PdfDictionary((KnownNames.Type, KnownNames.Catalog), (KnownNames.Outlines, outlines), (KnownNames.Pages,  PagesParent)));

            Creator.Add(catalog);
            Creator.Add(outlines);
            Creator.Add(PagesParent);
            
            Creator.AddToTrailerDictionary(KnownNames.Root, catalog);
        }

        private bool defaultObjectsSuppressed;

        private void AddDefaultObjects()
        {
            if (defaultObjectsSuppressed) return;
            WriteSingleObject(defaultFont);
            WriteSingleObject(defaultProcSet);
        }

        private void WriteSingleObject(LazyPdfObject lazy)
        {
            if (lazy.HasValue)
                Creator.Add(lazy.Value);
        }

        public void SuppressDefaultObjectWrite()
        {
            defaultObjectsSuppressed = true;
        }
    }
}