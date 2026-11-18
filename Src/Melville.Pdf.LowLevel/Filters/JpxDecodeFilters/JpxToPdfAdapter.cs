using System;
using System.IO;
using System.Threading.Tasks;
using CoreJ2K;
using Melville.Hacks;

using Melville.Pdf.LowLevel.Model.Objects;
using Melville.StreamInterfaces;

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
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context)
    {
        throw new System.NotSupportedException();
    }

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context)
    {
        //J2KiMAGE.FromStream just reads the whole stream into a byte Array.
        //Eventually We can do better by renting an array and using that.
        // we can also handle streams that do and do not know their length.
        try
        {
#warning be smarter than this about the buffer
            var stream = new MemoryStream();
            await input.CopyToAsync(stream).CA();
            input.Dispose();
            var bytes = stream.ToArray() ;

            var independentImage = J2kImage.FromBytes(bytes);
            if (context is IReportColorComponents colorSpace)
            {
                await colorSpace.ReportColorComponentsAsync(
                    independentImage.NumberOfComponents).CA();
            }
            return new ReadPartialBytesStream(independentImage,
                independentImage.NumberOfComponents);
        }
        catch (Exception)
        {
            return Stream.Null;
        }
        finally
        {
            input.Dispose();
        }
    }
}