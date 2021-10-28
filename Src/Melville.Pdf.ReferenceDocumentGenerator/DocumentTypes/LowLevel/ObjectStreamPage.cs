using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel
{
    public class ObjectStreamPage: CreatePdfParser
    {
        public ObjectStreamPage() : base("-ObjectStream", "Document using an object stream.")
        {
        }

        public override async ValueTask WritePdfAsync(Stream target) =>
            await (await FiltersAsync()).WriteToWithXrefStreamAsync(target);

        public static async ValueTask<PdfLowLevelDocument> FiltersAsync()
        {
            var creator = new PdfDocumentCreator();
            await using (creator.LowLevelCreator.ObjectStreamContext(
                             new DictionaryBuilder()))
            {
                var page = creator.Pages.CreatePageInObjectStream();
                page.AddStandardFont("F1", BuiltInFontName.Helvetica, FontEncodingName.WinAnsiEncoding);
                page.AddToContentStream(new DictionaryBuilder().AsStream(
                    "BT\n/F1 24 Tf\n100 700 Td\n(Uses Object Stream) Tj\nET\n"));
            }

            return creator.CreateDocument();
            // var builder = new PdfCreator(1, 7);
            // await using (builder.Creator.ObjectStreamContext(new DictionaryBuilder()))
            // {
            //     builder.Creator.Add(builder.DefaultFont);
            //     builder.Creator.Add(builder.DefaultProcSet);
            // }
            //     builder.SuppressDefaultObjectWrite();
            //     var page = builder.Creator.AsIndirectReference(
            //          builder.CreateUnattachedPage(new DictionaryBuilder()
            //              .AsStream("BT\n/F1 24 Tf\n100 700 Td\n(Uses Object Stream) Tj\nET\n")));
            //     builder.Creator.Add(await ObjectStreamCreation.NewObjectStream(page));
            //     builder.AddPageToPagesCollection(page);
            //     builder.FinalizePages();
//                return builder.Creator;
        }
    }
}