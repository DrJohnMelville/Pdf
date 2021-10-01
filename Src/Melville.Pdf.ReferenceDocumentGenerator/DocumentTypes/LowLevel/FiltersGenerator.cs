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

        public override async ValueTask WritePdfAsync(Stream target) =>
            await Filters().CreateDocument().WriteToWithXrefStreamAsync(target);

        public static ILowLevelDocumentCreator Filters()
        {
            var builder = new PdfCreator();
            builder.Creator.AddEncryption(new V4Encryptor("","", 128,
                PdfPermission.None, KnownNames.Identity, KnownNames.Identity,
                new V4CfDictionary(KnownNames.V2, 128/8)));
            CreatePage(builder, "Rc4 Crypt Filter", KnownNames.Crypt,
                new PdfDictionary((KnownNames.Type, KnownNames.CryptFilterDecodeParms),
                    (KnownNames.Name, KnownNames.StdCF)));
          CreatePage(builder, "Identity Crypt Filter", KnownNames.Crypt,
                new PdfDictionary((KnownNames.Type, KnownNames.CryptFilterDecodeParms),
                    (KnownNames.Name, KnownNames.Identity)));
            CreatePage(builder, "RunLength AAAAAAAAAAAAAAAAAAAAAA " + RandomString(9270),
                KnownNames.RunLengthDecode);
            CreatePage(builder, "LZW -- LateChange" + RandomString(9270), 
                KnownNames.LZWDecode, new PdfDictionary((KnownNames.EarlyChange, new PdfInteger(0))));
            CreatePage(builder, "LZW -- " + RandomString(9270), KnownNames.LZWDecode);
            CreatePage(builder, "Ascii Hex", KnownNames.ASCIIHexDecode);
            CreatePage(builder, "Ascii 85", KnownNames.ASCII85Decode);
            CreatePage(builder, "Flate Decode", KnownNames.FlateDecode);
            PredictionPage(builder, "Flate Decode With Tiff Predictor 2", 2 );
            PredictionPage(builder, "Flate Decode With Png Predictor 10", 10);
            PredictionPage(builder, "Flate Decode With Png Predictor 11", 11);
            PredictionPage(builder, "Flate Decode With Png Predictor 12", 12);
            PredictionPage(builder, "Flate Decode With Png Predictor 13", 13);
            PredictionPage(builder, "Flate Decode With Png Predictor 14", 14);
            PredictionPage(builder, "Flate Decode With Png Predictor 15", 15);

            builder.FinalizePages();
            return builder.Creator;
        }

        private static void CreatePage(PdfCreator builder, string Text, PdfName encoding,
            PdfObject? parameters = null)
        {
            builder.AddPageToPagesCollection(builder.Creator.Add(
                builder.CreateUnattachedPage(builder.Creator.Add( builder.Creator.NewCompressedStream(
                    $"BT\n/F1 24 Tf\n100 100 Td\n(({Text})) Tj\nET\n",
                    encoding,  parameters
                )))
            ));
        }

        private static void PredictionPage(
            PdfCreator builder, string text, int Predictor)
        {
             CreatePage(builder, text, KnownNames.FlateDecode,
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