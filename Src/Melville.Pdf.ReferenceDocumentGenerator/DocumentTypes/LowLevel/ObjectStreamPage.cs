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

        public override async ValueTask WritePdfAsync(Stream target) =>
            await (await FiltersAsync()).CreateDocument().WriteToWithXrefStreamAsync(target);

        public static async ValueTask<ILowLevelDocumentCreator> FiltersAsync()
        {
            var builder = new PdfCreator(1, 7);
                builder.Creator.Add(
                    await builder.Creator.NewObjectStream(builder.DefaultFont, 
                        builder.DefaultProcSet));
                builder.SuppressDefaultObjectWrite();
                var page = builder.Creator.AsIndirectReference(
                     builder.CreateUnattachedPage("BT\n/F1 24 Tf\n100 100 Td\n(Uses Object Stream) Tj\nET\n"));
                builder.Creator.Add(await builder.Creator.NewObjectStream(page));
                builder.AddPageToPagesCollection(page);
                builder.FinalizePages();
                return builder.Creator;
        }
    }
}