using System.Buffers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Melville.Pdf.Model.Renderers.Bitmaps;
using Melville.Pdf.Model.Renderers.Colors;

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
            using var bmpToMempry = new WriteableBitmapMemoryManager(ret);
            try
            {
                await bitmap.RenderPbgraAsync(bmpToMempry.Memory);
                ret.AddDirtyRect(new Int32Rect(0, 0, bitmap.Width, bitmap.Height));
            }
            catch (InvalidOperationException)
            {
                // if we cannot decode just print what we have
            }
            return ret;
   
        }
    }
}

internal class WriteableBitmapMemoryManager: MemoryManager<DeviceColor>
{
    private readonly WriteableBitmap source;
    public WriteableBitmapMemoryManager(WriteableBitmap source)
    {
        this.source = source;
        source.Lock();
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            source.Unlock();
        }
    }
    public unsafe override Span<DeviceColor> GetSpan() =>
        new Span<DeviceColor>(
            (DeviceColor*)source.BackBuffer.ToPointer(), source.PixelWidth * source.PixelHeight);
    public unsafe override MemoryHandle Pin(int elementIndex = 0) => throw new NotImplementedException();
  
    public override void Unpin()
    {
    }
}