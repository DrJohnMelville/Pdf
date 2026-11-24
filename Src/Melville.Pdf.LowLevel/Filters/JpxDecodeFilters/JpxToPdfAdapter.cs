using CoreJ2K;
using Melville.Hacks;
using Melville.Parsing.ReaderExtensions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Streams;
using System;
using System.Buffers;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.JpxDecodeFilters;

/// <summary>
/// If the decoding context is this interface, then the JpxToPdfAdapter will use this
/// to report the number of color components it is going to provide per pixel.
/// The ReconcilableColorSpace object uses this to revert to a better colorspace in the
/// error condition where a JPX image has too many or too few color components for its
/// color space
/// </summary>
public interface IReportColorComponents
{
    /// <summary>
    /// Informs the receiver of the number of color components that will be reported per pixel
    /// </summary>
    /// <param name="components">Number of components each pixel will generate</param>
    ValueTask ReportColorComponentsAsync(int components);
}

internal class JpxToPdfAdapter : ICodecDefinition
{
    public ValueTask<IPdfStreamReader> EncodeOnReadStreamAsync(
        IPdfStreamReader data, PdfDirectObject parameters, object? context) => 
        throw new NotSupportedException();

    public async ValueTask<IPdfStreamReader> DecodeOnReadStreamAsync(IPdfStreamReader input, PdfDirectObject parameters, object? context)
    {
        try
        {
            //J2KiMAGE.FromStream just reads the whole stream into a byte Array.  Since it has to be
            // done we can do it intelligently with rented arrays.
            var (bytes, length) = await new AsyncReaderToByteArray(input).ReadAsync().CA();

            // J2kImage special cases FromStream to extract the buffer out of a MemoryStream
            var independentImage = J2kImage.FromStream(new MemoryStream(bytes, 0, length, false, true));

            await TryReportColorComponents(context, independentImage);

            ArrayPool<byte>.Shared.Return(bytes);

            return new PdfMemoryReader(independentImage.GetUsedBytes());

        }
        catch (Exception)
        {
            return new PdfMemoryReader(Array.Empty<byte>());
        }
        finally
        {
            input.Dispose();
        }
    }

    private static async ValueTask 
        TryReportColorComponents(object? context, CoreJ2K.Util.InterleavedImage independentImage)
    {
        if (context is IReportColorComponents colorSpace)
        {
            await colorSpace.ReportColorComponentsAsync(
                independentImage.NumberOfComponents).CA();
        }
    }

}