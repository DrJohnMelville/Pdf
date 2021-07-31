using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel
{
    public class MinimalPdfParser: CreatePdfParser
    {
        public MinimalPdfParser() : base("-min")
        {
        }

        protected override ValueTask WritePdf(Stream target) =>
            new(MinimalPdf(1, 7).CreateDocument().WriteTo(target));

        public static ILowLevelDocumentCreator MinimalPdf(int major, int minor)
        {
            var builder = new LowLevelDocumentCreator();
            builder.SetVersion((byte)major, (byte)minor);
            var catalog = builder.AsIndirectReference();
            var outlines = builder.AsIndirectReference(builder.NewDictionary(
                (KnownNames.Type, KnownNames.Outlines), (KnownNames.Count, new PdfInteger(0))));
            var pages = builder.AsIndirectReference();
            var page = builder.AsIndirectReference();
            var stream = builder.AsIndirectReference(builder.NewStream("... Page0marking operators ..."));
            var procset = builder.AsIndirectReference(new PdfArray(KnownNames.PDF));
            builder.AssignValueToReference(page, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Page),
                (KnownNames.Parent, pages),
                (KnownNames.MediaBox, new PdfArray(
                    new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))),
                (KnownNames.Contents, stream),
                (KnownNames.Resources, builder.NewDictionary((KnownNames.ProcSet, procset)))));
            builder.AssignValueToReference(pages, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Pages),
                (KnownNames.Kids, page),
                (KnownNames.Count, new PdfInteger(1))
                ));
            builder.AssignValueToReference(catalog, builder.NewDictionary(
                (KnownNames.Type, KnownNames.Catalog),
                (KnownNames.Outlines, outlines),
                (KnownNames.Pages, pages)
                ));

            builder.Add(catalog);
            builder.Add(outlines);
            builder.Add(pages);
            builder.Add(page);
            builder.Add(stream);
            builder.Add(procset);
            
            builder.AddToTrailerDictionary(KnownNames.Root, catalog);
            return builder;       
        }
        // This is roughly correct, but I used the builder so all the offsets are correct.        
        //%PDF-{major}.{minor}  
        // 1 0 obj
        // << /Type /Catalog /Outlines 2 0 R /Pages 3 0 R >>
        // endobj
        // 2 0 obj
        // << /Type Outlines /Count 0 >>
        // endobj
        // 3 0 obj
        // << /Type /Pages /Kids [4 0 R] /Count 1 >>
        // endobj
        // 4 0 obj
        // << /Type /Page /Parent 3 0 R /MediaBox [0 0 612 792] /Contents 5 0 R /Resources << /ProcSet 6 0 R >> >>
        // endobj
        // 5 0 obj
        // << /Length 35 >>
        // stream
        // … Page-marking operators …
        // endstream 
        // endobj
        // 6 0 obj
        // [/PDF]
        // endobj
        // xref
        // 0 7
        // 0000000000 65535 f 
        // 0000000009 00000 n 
        // 0000000074 00000 n 
        // 0000000119 00000 n 
        // 0000000176 00000 n 
        // 0000000295 00000 n 
        // 0000000376 00000 n 
        // trailer 
        // << /Size 7 /Root 1 0 R >>
        // startxref
        // 418
        // %%EOF";
     }
}