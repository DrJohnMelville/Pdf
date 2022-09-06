using System.Windows;
using Melville.Pdf.LowLevelViewer.MainDisplay;

namespace Melville.Pdf.LowLevelViewer.Services;

public class CloseWpfApp:ICloseApp
{
    private readonly Application application;

    public CloseWpfApp(Application application)
    {
        this.application = application;
    }

    public void Close() => application.Shutdown();
}