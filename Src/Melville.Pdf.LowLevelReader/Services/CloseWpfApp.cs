using System.Windows;
using Melville.Pdf.LowLevelReader.MainDisplay;

namespace Melville.Pdf.LowLevelReader.Services;

public class CloseWpfApp:ICloseApp
{
    private readonly Application application;

    public CloseWpfApp(Application application)
    {
        this.application = application;
    }

    public void Close() => application.Shutdown();
}