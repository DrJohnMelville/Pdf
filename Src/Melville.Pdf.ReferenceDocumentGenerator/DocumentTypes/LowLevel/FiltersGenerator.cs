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
            return await SimplePdfShell.Generate(1, 7, async (builder, pages) =>
            {
                var procset = builder.Add(new PdfArray(KnownNames.PDF));
                var font = builder.Add(new PdfDictionary(
                    (KnownNames.Type, KnownNames.Font ),
                    (KnownNames.Subtype, KnownNames.Type1),
                    (KnownNames.Name, new PdfName("F1")),
                    (KnownNames.BaseFont, KnownNames.Helvetica),
                    (KnownNames.Encoding, KnownNames.MacRomanEncoding)));
                return new[]
                {
                    await CreatePage(builder, pages, procset, "RunLength AAAAAAAAAAAAAAAAAAAAAA "+RandomString(9270),
                        font, KnownNames.RunLengthDecode),
                    await CreatePage(builder, pages, procset, "LZW -- LateChange"+RandomString(9270), font, 
                        KnownNames.LZWDecode, new PdfDictionary((KnownNames.EarlyChange, new PdfInteger(0)))),
                    await CreatePage(builder, pages, procset, "LZW -- "+RandomString(9270), font, KnownNames.LZWDecode),
                    await CreatePage(builder, pages, procset,"Ascii Hex", font, KnownNames.ASCIIHexDecode),
                    await CreatePage(builder, pages, procset, "Ascii 85", font, KnownNames.ASCII85Decode),
                    await CreatePage(builder, pages, procset, "Flate Decode", font, KnownNames.FlateDecode),
                    await PredictionPage(builder, pages, procset, font, "Flate Decode With Tiff Predictor 2", 2),
                    await PredictionPage(builder, pages, procset, font, "Flate Decode With Png Predictor 10", 10),
                    await PredictionPage(builder, pages, procset, font, "Flate Decode With Png Predictor 11", 11),
                    await PredictionPage(builder, pages, procset, font, "Flate Decode With Png Predictor 12", 12),
                    await PredictionPage(builder, pages, procset, font, "Flate Decode With Png Predictor 13", 13),
                    await PredictionPage(builder, pages, procset, font, "Flate Decode With Png Predictor 14", 14),
                    await PredictionPage(builder, pages, procset, font, "Flate Decode With Png Predictor 15", 15),
                };
            });
        }

        private static async Task<PdfIndirectReference> PredictionPage(
            ILowLevelDocumentCreator builder, PdfIndirectReference pages, 
            PdfIndirectReference procset, PdfIndirectReference font, string text, int Predictor)
        {
            return await CreatePage(builder, pages, procset, text, font, KnownNames.FlateDecode,
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

        private static async ValueTask<PdfIndirectReference> CreatePage(
            ILowLevelDocumentCreator builder, 
            PdfIndirectReference pages, 
            PdfIndirectReference procset,
            string text,
            PdfIndirectReference font,
            PdfObject filters , PdfObject? parameters = null)
        {
            var stream = builder.Add(
                await builder.NewCompressedStream($"BT\n/F1 24 Tf\n100 100 Td\n({text}) Tj\nET\n",
                    filters, parameters));
            return builder.Add(new PdfDictionary(
                (KnownNames.Type, KnownNames.Page), 
                (KnownNames.Parent, pages),
                (KnownNames.MediaBox, new PdfArray(
                    new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))), 
                (KnownNames.Contents, stream), 
                (KnownNames.Resources, new PdfDictionary(
                    (KnownNames.Font, new PdfDictionary(
                        (new PdfName("F1"), font))), 
                    (KnownNames.ProcSet, procset)))));
        }
    }
}