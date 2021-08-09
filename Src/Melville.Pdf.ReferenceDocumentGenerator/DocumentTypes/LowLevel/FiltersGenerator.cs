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
        public FiltersGenerator() : base("-filters")
        {
        }

        protected override ValueTask WritePdf(Stream target) =>
            new(Filters().CreateDocument().WriteTo(target));

        public static ILowLevelDocumentCreator Filters()
        {
            return SimplePdfShell.Generate(1, 7, (builder, pages) =>
            {
                var procset = builder.Add(new PdfArray(KnownNames.PDF));
                var font = builder.Add(builder.NewDictionary(
                        (KnownNames.Type, KnownNames.Font ),
                        (KnownNames.Subtype, KnownNames.Type1),
                        (KnownNames.Name, new PdfName("F1")),
                        (KnownNames.BaseFont, KnownNames.Helvetica),
                        (KnownNames.Encoding, KnownNames.MacRomanEncoding)
                        ));
                return new[]
                {
                    CreatePage(builder, pages, procset,"Ascii Hex", font, KnownNames.ASCIIHexDecode),
                    CreatePage(builder, pages, procset, "Ascii 85", font, KnownNames.ASCII85Decode),
                    CreatePage(builder, pages, procset, "FlateDecode", font, KnownNames.FlateDecode),
                    CreatePage(builder, pages, procset, "FL", font, KnownNames.Fl),
                    CreatePage(builder, pages, procset, "LZW -- "+RandomString(350), font, KnownNames.LZWDecode),
                };
            });
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

        private static PdfIndirectReference CreatePage(
            ILowLevelDocumentCreator builder, 
            PdfIndirectReference pages, 
            PdfIndirectReference procset,
            string text,
            PdfIndirectReference font,
            PdfObject filters )
        {
            var stream = builder.Add(
                builder.NewCompressedStream($"BT\n/F1 24 Tf\n100 100 Td\n({text}) Tj\nET\n",
                    filters));
            return builder.Add(builder.NewDictionary(
                (KnownNames.Type, KnownNames.Page),
                (KnownNames.Parent, pages),
                (KnownNames.MediaBox, new PdfArray(
                    new PdfInteger(0), new PdfInteger(0), new PdfInteger(612), new PdfInteger(792))),
                (KnownNames.Contents, stream),
                (KnownNames.Resources, builder.NewDictionary(
                    (KnownNames.Font, builder.NewDictionary((new PdfName("F1"), font))),
                    (KnownNames.ProcSet, procset)))));
        }
    }
}