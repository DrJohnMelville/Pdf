using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel
{
    public class FiltersGenerator: CreatePdfParser
    {
        public FiltersGenerator() : base("-filters")
        {
        }

        protected override ValueTask WritePdf(Stream target) =>
            new(Filters().CreateDocument().WriteTo(target));

        public static ILowLevelDocumentCreator Filters()
        {
            return SimplePdfShell.Generate(1, 7, (builder, pages) =>
            {
                var stream = builder.Add(builder.NewStream("... Page0marking operators ..."));
                var procset = builder.Add(new PdfArray(KnownNames.PDF));
                var page1 = CreatePage(builder, pages, stream, procset);
                var page2 = CreatePage(builder, pages, stream, procset);
                return new[] {page1, page2};
            });
        }

        private static PdfIndirectReference CreatePage(ILowLevelDocumentCreator builder, PdfIndirectReference pages, PdfIndirectReference stream, PdfIndirectReference procset)
        {
            return builder.Add(builder.NewDictionary(
                (KnownNames.Type, KnownNames.Page),
                (KnownNames.Parent, pages),
                (KnownNames.MediaBox, new PdfArray(
                    new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))),
                (KnownNames.Contents, stream),
                (KnownNames.Resources, builder.NewDictionary((KnownNames.ProcSet, procset)))));
        }
    }
}