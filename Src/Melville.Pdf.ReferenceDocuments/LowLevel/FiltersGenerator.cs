namespace Melville.Pdf.ReferenceDocuments.LowLevel;

public class FiltersGenerator : CreatePdfParser
{
    public FiltersGenerator() : base("-filters", "Document using all filter types.")
    {
    }

    public override async ValueTask WritePdfAsync(Stream target) =>
        await Filters().WriteToWithXrefStreamAsync(target);

    public static PdfLowLevelDocument Filters()
    {
        var builder = new PdfDocumentCreator();
        builder.Pages.AddStandardFont("F1", BuiltInFontName.Helvetica, FontEncodingName.StandardEncoding);

        BuildEncryptedDocument.AddEncryption(builder.LowLevelCreator, new V4Encryptor("", "", 128,
            PdfPermission.None, KnownNames.Identity, KnownNames.Identity,
            new V4CfDictionary(KnownNames.V2, 128 / 8)));

        CreatePage(builder, "Rc4 Crypt Filter", KnownNames.Crypt,
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.CryptFilterDecodeParms)
                .WithItem(KnownNames.Name, KnownNames.StdCF)
                .AsDictionary());
        CreatePage(builder, "Identity Crypt Filter", KnownNames.Crypt,
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.CryptFilterDecodeParms)
                .WithItem(KnownNames.Name, KnownNames.Identity)
                .AsDictionary());
        CreatePage(builder, "RunLength AAAAAAAAAAAAAAAAAAAAAA " + RandomString(9270),
            KnownNames.RunLengthDecode);
        CreatePage(builder, "LZW -- LateChange" + RandomString(9270), KnownNames.LZWDecode,
            new DictionaryBuilder().WithItem(KnownNames.EarlyChange, new PdfInteger(0)).AsDictionary());
        CreatePage(builder, "LZW -- " + RandomString(9270), KnownNames.LZWDecode);
        CreatePage(builder, "Ascii Hex", KnownNames.ASCIIHexDecode);
        CreatePage(builder, "Ascii 85", KnownNames.ASCII85Decode);
        CreatePage(builder, "Flate Decode", KnownNames.FlateDecode);
        PredictionPage(builder, "Flate Decode With Tiff Predictor 2", 2);
        PredictionPage(builder, "Flate Decode With Png Predictor 10", 10);
        PredictionPage(builder, "Flate Decode With Png Predictor 11", 11);
        PredictionPage(builder, "Flate Decode With Png Predictor 12", 12);
        PredictionPage(builder, "Flate Decode With Png Predictor 13", 13);
        PredictionPage(builder, "Flate Decode With Png Predictor 14", 14);
        PredictionPage(builder, "Flate Decode With Png Predictor 15", 15);

        return builder.CreateDocument();
    }

    private static void CreatePage(PdfDocumentCreator builder, string Text, FilterName encoding,
        PdfObject? parameters = null)
    {
        builder.Pages.CreatePage().AddToContentStream(
            new DictionaryBuilder().WithFilter(encoding).WithFilterParam(parameters)
                .AsStream($"BT\n/F1 24 Tf\n100 700 Td\n(({Text})) Tj\nET\n")
        );
    }

    private static void PredictionPage(PdfDocumentCreator builder, string text, int Predictor)
    {
        CreatePage(builder, text, KnownNames.FlateDecode,
            new DictionaryBuilder()
                .WithItem(KnownNames.Colors, new PdfInteger(2))
                .WithItem(KnownNames.Columns, new PdfInteger(5))
                .WithItem(KnownNames.Predictor, new PdfInteger(Predictor))
                .AsDictionary());
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