using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.LowLevel;

public class FiltersGenerator : CreatePdfParser
{
    public FiltersGenerator() : base("Document using all filter types.")
    {
    }

    public override async ValueTask WritePdfAsync(Stream target) =>
        await (await Filters()).WriteToWithXrefStreamAsync(target);

    public async ValueTask<PdfLowLevelDocument> Filters()
    {
        var builder = new PdfDocumentCreator();
        builder.Pages.AddStandardFont("F1", BuiltInFontName.Helvetica, FontEncodingName.StandardEncoding);

        BuildEncryptedDocument.AddEncryption(builder.LowLevelCreator, new V4Encryptor("", "", 128,
            PdfPermission.None, KnownNames.Identity, KnownNames.Identity,
            new V4CfDictionary(KnownNames.V2, 128 / 8)));

        await CreatePage(builder, "Rc4 Crypt Filter", FilterName.Crypt,
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.CryptFilterDecodeParms)
                .WithItem(KnownNames.Name, KnownNames.StdCF)
                .AsDictionary());
        await CreatePage(builder, "Identity Crypt Filter", FilterName.Crypt,
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.CryptFilterDecodeParms)
                .WithItem(KnownNames.Name, KnownNames.Identity)
                .AsDictionary());
        await CreatePage(builder, "RunLength AAAAAAAAAAAAAAAAAAAAAA " + RandomString(9270),
            FilterName.RunLengthDecode);
        await CreatePage(builder, "LZW -- LateChange" + RandomString(9270), FilterName.LZWDecode,
            new DictionaryBuilder().WithItem(KnownNames.EarlyChange, new PdfInteger(0)).AsDictionary());
        await CreatePage(builder, "LZW -- " + RandomString(9270), FilterName.LZWDecode);
        await CreatePage(builder, "Ascii Hex", FilterName.ASCIIHexDecode);
        await CreatePage(builder, "Ascii 85", FilterName.ASCII85Decode);
        await CreatePage(builder, "Flate Decode", FilterName.FlateDecode);
        await PredictionPage(builder, "Flate Decode With Tiff Predictor 2", 2);
        await PredictionPage(builder, "Flate Decode With Png Predictor 10", 10);
        await PredictionPage(builder, "Flate Decode With Png Predictor 11", 11);
        await PredictionPage(builder, "Flate Decode With Png Predictor 12", 12);
        await PredictionPage(builder, "Flate Decode With Png Predictor 13", 13);
        await PredictionPage(builder, "Flate Decode With Png Predictor 14", 14);
        await PredictionPage(builder, "Flate Decode With Png Predictor 15", 15);

        return builder.CreateDocument();
    }

    private static ValueTask CreatePage(PdfDocumentCreator builder, string Text, FilterName encoding,
        PdfObject? parameters = null) =>
        builder.Pages.CreatePage().AddToContentStreamAsync(
            new DictionaryBuilder().WithFilter(encoding).WithFilterParam(parameters),
            i => {
                i.SetFont(NameDirectory.Get("F1"), 24);
                using var block = i.StartTextBlock();
                block.MovePositionBy(100, 700);
                block.ShowString(Text);
                return ValueTask.CompletedTask;
            });

    private static ValueTask PredictionPage(PdfDocumentCreator builder, string text, int Predictor) =>
        CreatePage(builder, text, FilterName.FlateDecode,
            new DictionaryBuilder()
                .WithItem(KnownNames.Colors, new PdfInteger(2))
                .WithItem(KnownNames.Columns, new PdfInteger(5))
                .WithItem(KnownNames.Predictor, new PdfInteger(Predictor))
                .AsDictionary());


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