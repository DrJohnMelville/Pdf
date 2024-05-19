using Melville.Fonts;
using Melville.Fonts.SfntParsers;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public class MultiFontViewModel
{
      public IReadOnlyList<SingleFontViewModel> Fonts { get; }

      public MultiFontViewModel(IReadOnlyList<IGenericFont> fonts)
      {
          Fonts = fonts.Select(ViewFactory).ToList();
      }

      private SingleFontViewModel ViewFactory(IGenericFont arg) => arg switch
          {
              SFnt sfnt => new SfntViewModel(sfnt),
              _ => new SingleFontViewModel(arg)
          };
}