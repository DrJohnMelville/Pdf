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

        protected override async ValueTask WritePdf(Stream target) =>
            await ( await MinimalPdf(1, 7)).CreateDocument().WriteTo(target);

        public static async ValueTask<ILowLevelDocumentCreator> MinimalPdf(int major, int minor)
        {
            return await SimplePdfShell.Generate(major, minor, async (builder, pages) =>
            {
                var page = builder.AsIndirectReference();
                var stream = builder.Add(builder.NewStream("... Page0marking operators ..."));
                var procset = builder.Add(new PdfArray(KnownNames.PDF));
                builder.AssignValueToReference(page, builder.NewDictionary(
                    (KnownNames.Type, KnownNames.Page),
                    (KnownNames.Parent, pages),
                    (KnownNames.MediaBox, new PdfArray(
                        new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))),
                    (KnownNames.Contents, stream),
                    (KnownNames.Resources, builder.NewDictionary((KnownNames.ProcSet, procset)))));
                builder.Add(page);
                return new[] {page};
            });
        }
     }
}