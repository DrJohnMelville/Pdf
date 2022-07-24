using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Melville.Pdf.Wpf.DebugMethods;

public class ImagePreviewWindow : Window
{
    public void Create(BitmapSource bmp)
    {
        Height = bmp.PixelHeight + 50;
        Width = bmp.PixelWidth;
        Content = new StackPanel()
        {
            Children =
            {
                new Border
                {
                    BorderBrush = Brushes.Red, BorderThickness = new Thickness(1),
                    Background = Brushes.LightSlateGray,
                    Child = Image(bmp)
                },
                CreateCloseButton()
            }
        };
    }

    private static Image Image(BitmapSource bmp)
    {
        var ret = new Image();
        ret.BeginInit();
        ret.Source = bmp;
        ret.EndInit();
        return ret;
    }

    private Button CreateCloseButton()
    {
        var ret = new Button() { Content = "Close", };
        ret.Click += (s, e) => Close();
        return ret;
    }
}