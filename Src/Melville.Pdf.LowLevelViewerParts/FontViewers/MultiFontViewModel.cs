using Melville.Fonts;
using Melville.Parsing.ParserMapping;
using Melville.Pdf.LowLevelViewerParts.ParseMapViews;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public class MultiFontViewModel
{
      public IReadOnlyList<object?> Fonts { get; }

      public MultiFontViewModel(IReadOnlyList<IGenericFont> fonts, ParseMap map)
      {
          Fonts = fonts.Select(ViewFactory)
              .OfType<object>()
              .Append(new ParseMapViewModel(map))
              .ToList();
      }

      private object? ViewFactory(IGenericFont arg) => arg.CreateSpecificViewModel();
}