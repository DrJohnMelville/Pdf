using Melville.Fonts;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public class MultiFontViewModel
{
      public IReadOnlyList<object?> Fonts { get; }

      public MultiFontViewModel(IReadOnlyList<IGenericFont> fonts)
      {
          Fonts = fonts.Select(ViewFactory).ToList();
      }

      private object? ViewFactory(IGenericFont arg) => arg.CreateSpecificViewModel();
}