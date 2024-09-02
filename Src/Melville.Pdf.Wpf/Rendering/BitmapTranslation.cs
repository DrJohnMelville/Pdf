using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Pdf.Model.Renderers.Bitmaps;

namespace Melville.Pdf.Wpf.Rendering
{
    /// <summary>
    /// Render a IPdfBitmap as a WPF compatible BitmapSource
    /// </summary>
    public static class BitmapTranslation
    {
        /// <summary>
        /// Render an IPdfBitmap into a BitmapSource
        /// </summary>
        /// <param name="bitmap">The bitmap to render</param>
        /// <returns>A bitmap CodeSource representing the rendered bitmap.</returns>
        public static async ValueTask<BitmapSource> ToWpfBitmapAsync(this IPdfBitmap bitmap)
        {
            var ret = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Pbgra32, null);
            ret.Lock();
            try
            {
                await FillBitmapAsync(bitmap, ret);
            }
            catch (InvalidOperationException)
            {
                // if we cannot decode just print what we have
            }
            finally
            {
                ret.AddDirtyRect(new Int32Rect(0,0,bitmap.Width, bitmap.Height));
                ret.Unlock();
            }
            return ret;
   
        }
        private static unsafe ValueTask FillBitmapAsync(IPdfBitmap bitmap, WriteableBitmap wb) =>
            bitmap.RenderPbgraAsync((byte*)wb.BackBuffer.ToPointer());
    }
}