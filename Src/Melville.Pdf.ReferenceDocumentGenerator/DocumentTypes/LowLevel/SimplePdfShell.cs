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
                new DictionaryBuilder()
                    .WithItem(KnownNames.Type, KnownNames.Font ) 
                    .WithItem(KnownNames.Subtype, KnownNames.Type1) 
                    .WithItem(KnownNames.Name, new PdfName("F1")) 
                    .WithItem(KnownNames.BaseFont, KnownNames.Helvetica) 
                    .WithItem(KnownNames.Encoding, KnownNames.MacRomanEncoding)
                    .AsDictionary()
                );
        }

        public void AddPageToPagesCollection(PdfObject page) => 
            pages.Add(Creator.AsIndirectReference(page));

        public PdfDictionary CreateUnattachedPage(PdfIndirectReference content) => new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.Page)
            .WithItem(KnownNames.Parent, PagesParent)
            .WithItem(KnownNames.MediaBox, 
                new PdfArray(new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792)))
            .WithItem(KnownNames.Contents, content)
            .WithItem(KnownNames.Resources,
                    new DictionaryBuilder()
                        .WithItem(KnownNames.Font, 
                            new DictionaryBuilder().WithItem(new PdfName("F1"), DefaultFont).AsDictionary())
                        .WithItem(KnownNames.ProcSet, DefaultProcSet)
                        .AsDictionary())
            .AsDictionary();
        
        public PdfDictionary CreateUnattachedPage(PdfStream source) =>
            CreateUnattachedPage(
                Creator.Add(source));

        public void CreateAttachedPage(PdfStream source)
        {
            var page = CreateUnattachedPage(source);
            AddPageToPagesCollection(Creator.Add(page));
        }

        public void FinalizePages()
        {
            AddDefaultObjects();
            var catalog = Creator.AsIndirectReference();
            var outlines = Creator.AsIndirectReference(new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Outlines)
                .WithItem(KnownNames.Count, new PdfInteger(0))
                .AsDictionary());
            Creator.AssignValueToReference(PagesParent, new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Pages)
                .WithItem(KnownNames.Kids, new PdfArray(pages))
                .WithItem(KnownNames.Count, new PdfInteger(pages.Count))
                .AsDictionary());
            Creator.AssignValueToReference(catalog, new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.Catalog)
                .WithItem(KnownNames.Outlines, outlines)
                .WithItem(KnownNames.Pages,  PagesParent)
                .AsDictionary());

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