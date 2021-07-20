using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.MVVM.Wpf.DiParameterSources;
using Melville.MVVM.Wpf.MvvmDialogs;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevelReader.DocumentParts;

namespace Melville.Pdf.LowLevelReader.MainDisplay
{
    public interface ICloseApp
    {
        public void Close();
    }
    [OnDisplayed(nameof(OpenFile))]
    public partial class MainDisplayViewModel
    {
        [AutoNotify] private DocumentPart[] root = Array.Empty<DocumentPart>();

        public async Task OpenFile([FromServices]IOpenSaveFile dlg, [FromServices]IPartParser parser, 
            [FromServices] ICloseApp closeApp)
        {
            var file = 
                dlg.GetLoadFile(null, "pdf", "Portable Document Format (*.pdf)|*.pdf", "File to open");
            if (file != null)
            {
                Root = await parser.Parse(file);
            }
            else
            {
                closeApp.Close();
            }
        }
    }
}