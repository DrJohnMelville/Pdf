using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel
{
    public class ObjectStreamPage: CreatePdfParser
    {
        public ObjectStreamPage() : base("-ObjectStream", "Document using an object stream.")
        {
        }

        protected override async ValueTask WritePdf(Stream target) =>
            await (await Filters()).CreateDocument().WriteToWithXrefStream(target);

        public static async ValueTask<ILowLevelDocumentCreator> Filters()
        {
            var builder = new PdfCreator(1, 7);
                builder.Creator.Add(await builder.Creator.NewObjectStream(builder.DefaultFont, builder.DefaultProcSet));
                var stream = builder.Creator.Add(
                    await builder.Creator.NewCompressedStream($"BT\n/F1 24 Tf\n100 100 Td\n({"Uses Object Stream"}) Tj\nET\n",
                        KnownNames.FlateDecode));
               
                var page = builder.Creator.AsIndirectReference(new PdfDictionary(
                    (KnownNames.Type, KnownNames.Page),
                    (KnownNames.Parent, builder.PagesParent), (KnownNames.MediaBox, new PdfArray(
                        new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))), 
                    (KnownNames.Contents, stream), 
                    (KnownNames.Resources,
                        new PdfDictionary(
                            (KnownNames.Font, new PdfDictionary((new PdfName("F1"), builder.DefaultFont))),
                            (KnownNames.ProcSet, builder.DefaultProcSet)))));
                builder.Creator.Add(await builder.Creator.NewObjectStream(page));
                builder.AddPage(page);
                builder.FinalizePages();
                return builder.Creator;

        }
    }
}