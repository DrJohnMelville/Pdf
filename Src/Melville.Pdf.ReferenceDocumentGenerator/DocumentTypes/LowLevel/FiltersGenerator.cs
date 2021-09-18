using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public FiltersGenerator() : base("-filters", "Document using all filter types.")
        {
        }

        protected override async ValueTask WritePdf(Stream target) =>
            await (await Filters()).CreateDocument().WriteToWithXrefStream(target);

        public static async ValueTask<ILowLevelDocumentCreator> Filters()
        {
            var builder = new PdfCreator();
            builder.Creator.Add(builder.DefaultFont);
            builder.Creator.Add(builder.DefaultProcSet);
            await CreatePage(builder, "RunLength AAAAAAAAAAAAAAAAAAAAAA " + RandomString(9270),
                KnownNames.RunLengthDecode);
            await CreatePage(builder, "LZW -- LateChange" + RandomString(9270), 
                KnownNames.LZWDecode, new PdfDictionary((KnownNames.EarlyChange, new PdfInteger(0))));
            await CreatePage(builder, "LZW -- " + RandomString(9270), KnownNames.LZWDecode);
            await CreatePage(builder, "Ascii Hex", KnownNames.ASCIIHexDecode);
            await CreatePage(builder, "Ascii 85", KnownNames.ASCII85Decode);
            await CreatePage(builder, "Flate Decode", KnownNames.FlateDecode);
            await PredictionPage(builder, "Flate Decode With Tiff Predictor 2", 2 );
            await PredictionPage(builder, "Flate Decode With Png Predictor 10", 10);
            await PredictionPage(builder, "Flate Decode With Png Predictor 11", 11);
            await PredictionPage(builder, "Flate Decode With Png Predictor 12", 12);
            await PredictionPage(builder, "Flate Decode With Png Predictor 13", 13);
            await PredictionPage(builder, "Flate Decode With Png Predictor 14", 14);
            await PredictionPage(builder, "Flate Decode With Png Predictor 15", 15);

            builder.FinalizePages();
            return builder.Creator;
        }

        private static async ValueTask CreatePage(PdfCreator builder, string Text, PdfName encoding,
            PdfObject? parameters = null)
        {
            builder.AddPageToPagesCollection(builder.Creator.Add(
                builder.CreateUnattachedPage(builder.Creator.Add( await builder.Creator.NewCompressedStream(
                    $"BT\n/F1 24 Tf\n100 100 Td\n(({Text})) Tj\nET\n",
                    encoding,  parameters
                )))
            ));
        }

        private static ValueTask PredictionPage(
            PdfCreator builder, string text, int Predictor)
        {
            return CreatePage(builder, text, KnownNames.FlateDecode,
                new PdfDictionary(
                    (KnownNames.Colors, new PdfInteger(2)), 
                    (KnownNames.Columns, new PdfInteger(5)),
                    (KnownNames.Predictor, new PdfInteger(Predictor))));
        }
        
        
        private static string RandomString(int length)
        {
            var rnd = new Random(10);
            var ret = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                ret.Append('A' + rnd.Next(26));
            }

            return ret.ToString();
        }
    }
}