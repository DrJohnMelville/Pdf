using System;
using System.IO;
using System.Threading.Tasks;
using CoreJ2K;
using CoreJ2K.Util;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;

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
        //4/27/2024 the J2kReader has a race bug that occasionally deadlocks reads.  The
        //memory buffer does not deadlock.
        var buffer = new byte[input.Length];
        await buffer.FillBufferAsync(0, (int)input.Length, input).CA();
        await input.DisposeAsync().CA();
        try
        {
            var independentImage = J2kImage.FromStream(new MemoryStream(buffer));
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
    }
}