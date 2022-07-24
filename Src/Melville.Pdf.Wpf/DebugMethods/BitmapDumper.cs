using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Melville.Pdf.Wpf.DebugMethods;

public static class BitmapDumper
{
    [Conditional("DEBUG")]
    public static void DumpBitmap(this BitmapSource bmp)
    {
        var win = new ImagePreviewWindow();
        win.Create(bmp);        
        win.Show();
    }
    [Conditional("DEBUG")]
    public static void DumpBitmapAndWait(this BitmapSource bmp)
    {
        var win = new ImagePreviewWindow();
        win.Create(bmp);        
        win.ShowDialog();
    }
}