using System.Collections.Generic;
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
        public MinimalPdfParser() : base("-min", "Minimal pdf with a single blank page.")
        {
        }

        protected override async ValueTask WritePdf(Stream target) =>
            await ( await MinimalPdf(1, 7)).CreateDocument().WriteTo(target);

        public static async ValueTask<ILowLevelDocumentCreator> MinimalPdf(int major, int minor)
        {
            var builder = new PdfCreator(major, minor);
            await builder.CreateAttachedPage("");
            builder.FinalizePages();
            return builder.Creator;
        }
     }
}