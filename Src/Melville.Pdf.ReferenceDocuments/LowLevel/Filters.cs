using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.LowLevel;

internal class FiltersGenerator : CreatePdfParser
{
    public FiltersGenerator() : base("Document using all filter types.")
    {
    }

    public override async ValueTask WritePdfAsync(Stream target) =>
        await (await FiltersAsync()).WriteToWithXrefStreamAsync(target);

    public async ValueTask<PdfLowLevelDocument> FiltersAsync()
    {
        var builder = new PdfDocumentCreator();
        builder.Pages.AddStandardFont("F1", BuiltInFontName.Helvetica, FontEncodingName.StandardEncoding);

        BuildEncryptedDocument.AddEncryption(builder.LowLevelCreator, 
            
            DocumentEncryptorFactory.V4("","", PdfPermission.None, 
            KnownNames.Identity, KnownNames.Identity, KnownNames.Identity,
            new V4CfDictionary(KnownNames.V2, 128 / 8)));

        await CreatePageAsync(builder, "Rc4 Crypt Filter", FilterName.Crypt,
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.CryptFilterDecodeParms)
                .WithItem(KnownNames.Name, KnownNames.StdCF)
                .AsDictionary());
        await CreatePageAsync(builder, "Identity Crypt Filter", FilterName.Crypt,
            new DictionaryBuilder()
                .WithItem(KnownNames.Type, KnownNames.CryptFilterDecodeParms)
                .WithItem(KnownNames.Name, KnownNames.Identity)
                .AsDictionary());
        await CreatePageAsync(builder, "RunLength AAAAAAAAAAAAAAAAAAAAAA " + RandomString(9270),
            FilterName.RunLengthDecode);
        await CreatePageAsync(builder, "LZW -- LateChange" + RandomString(9270), FilterName.LZWDecode,
            new DictionaryBuilder().WithItem(KnownNames.EarlyChange, 0).AsDictionary());
        await CreatePageAsync(builder, "LZW -- " + RandomString(9270), FilterName.LZWDecode);
        await CreatePageAsync(builder, "Ascii Hex", FilterName.ASCIIHexDecode);
        await CreatePageAsync(builder, "Ascii 85", FilterName.ASCII85Decode);
        await CreatePageAsync(builder, "Flate Decode", FilterName.FlateDecode);
        await PredictionPageAsync(builder, "Flate Decode With Tiff Predictor 2", 2);
        await PredictionPageAsync(builder, "Flate Decode With Png Predictor 10", 10);
        await PredictionPageAsync(builder, "Flate Decode With Png Predictor 11", 11);
        await PredictionPageAsync(builder, "Flate Decode With Png Predictor 12", 12);
        await PredictionPageAsync(builder, "Flate Decode With Png Predictor 13", 13);
        await PredictionPageAsync(builder, "Flate Decode With Png Predictor 14", 14);
        await PredictionPageAsync(builder, "Flate Decode With Png Predictor 15", 15);

        return builder.CreateDocument();
    }

    private static ValueTask CreatePageAsync(PdfDocumentCreator builder, string Text, FilterName encoding,
        PdfObject? parameters = null) =>
        builder.Pages.CreatePage().AddToContentStreamAsync(
            new DictionaryBuilder().WithFilter(encoding).WithFilterParam(parameters),
            i => {
                i.SetFontAsync(NameDirectory.Get("F1"), 24);
                using var block = i.StartTextBlock();
                block.MovePositionBy(100, 700);
                block.ShowStringAsync(Text);
                return ValueTask.CompletedTask;
            });

    private static ValueTask PredictionPageAsync(PdfDocumentCreator builder, string text, int Predictor) =>
        CreatePageAsync(builder, text, FilterName.FlateDecode,
            new DictionaryBuilder()
                .WithItem(KnownNames.Colors, 2)
                .WithItem(KnownNames.Columns, 5)
                .WithItem(KnownNames.Predictor, Predictor)
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