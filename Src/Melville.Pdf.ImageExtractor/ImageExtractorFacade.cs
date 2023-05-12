using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Melville.Pdf.Model.Renderers.DocumentRenderers;

namespace Melville.Pdf.ImageExtractor;

/// <summary>
/// This is the public facing interface to get images from a page or PDF File
/// </summary>
public static class ImageExtractorFacade
{
    /// <summary>
    /// Extract the Images from a given PDF page
    /// </summary>
    /// <param name="dr">DocumentRenderer than holds this document.</param>
    /// <param name="page">the one based page number to take images from</param>
    /// <returns>A list of images that appear on the page</returns>
    public static async ValueTask<IList<IExtractedBitmap>> 
        ImagesFrom(this DocumentRenderer dr, int page)
    {
        var ret = new List<IExtractedBitmap>();
        await dr.RenderPageTo(page, (rect, matrix) =>
        {
            var (width, height) = dr.ScalePageToRequestedSize(rect, new Vector2(-1, -1));
            var target = new ImageExtractorTarget(ret, page);
            dr.InitializeRenderTarget(target, rect, width, height, matrix);
            return target;
        });
        return ret;
    }
}