using Melville.Pdf.LowLevel.Model.Wrappers;

namespace Melville.Pdf.ReferenceDocuments.Text;

public class StandardFonts: CreatePdfParser
{
    public StandardFonts() : base("Display the 14 standard fonts")
    {
    }

    public override async ValueTask WritePdfAsync(Stream target)
    {
        var creator = new PdfDocumentCreator();
        var p1 = creator.Pages.CreatePage();
        creator.Pages.AddBox(BoxName.MediaBox, new PdfRect(0, 0, 612, 792));
        int ypos = 775;
        await p1.AddToContentStreamAsync(i =>
        {
            foreach (var font in AllFonts())
            {
                using var block = i.StartTextBlock();
                creator.Pages.AddStandardFont(font, font, FontEncodingName.WinAnsiEncoding);
                i.SetFont(font, 24);
                block.MovePositionBy(100, ypos);
                block.ShowString($"This is {(PdfName)font}");
                ypos -= 25;
            }
            
        });
        await LowLevelDocumentWriterOperations.WriteToAsync(creator.CreateDocument(), target);
    }

    private IEnumerable<BuiltInFontName> AllFonts() => new BuiltInFontName[]
    {
        BuiltInFontName.Courier, 
        BuiltInFontName.CourierBold, 
        BuiltInFontName.CourierOblique, 
        BuiltInFontName.CourierBoldOblique, 
        BuiltInFontName.Helvetica, 
        BuiltInFontName.HelveticaBold, 
        BuiltInFontName.HelveticaOblique, 
        BuiltInFontName.HelveticaBoldOblique, 
        BuiltInFontName.TimesRoman, 
        BuiltInFontName.TimesBold, 
        BuiltInFontName.TimesOblique,
        BuiltInFontName.TimesBoldOblique,
        BuiltInFontName.Symbol, 
        BuiltInFontName.ZapfDingbats,
    };
}