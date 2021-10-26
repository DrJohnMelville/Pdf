using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.Model.Creators;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;

namespace Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.HighLevel
{
    public class StandardFonts: CreatePdfParser
    {
        public StandardFonts() : base("-StdFont", "Display the 14 standard fonts")
        {
        }

        public override ValueTask WritePdfAsync(Stream target)
        {
            var creator = new PdfDocumentCreator();
            var p1 = creator.Pages.CreatePage();
            creator.Pages.AddBox(BoxName.MediaBox, new PdfRect(0, 0, 612, 792));
            int ypos = 775;
            var content = new StringBuilder();
            foreach (var font in AllFonts())
            {
                creator.Pages.AddStandardFont(font, font, FontEncodingName.WinAnsiEncoding);
                content.Append($"BT\n{font} 24 Tf\n 100 {ypos} Td\n (This is {font}) Tj\nET\n");
                ypos -= 25;
            }
            p1.AddToContentStream(content.ToString());
            return new(creator.CreateDocument().WriteToAsync(target));
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
}