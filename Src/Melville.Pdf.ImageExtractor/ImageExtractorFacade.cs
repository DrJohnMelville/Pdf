using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.ImageExtractor.ImageCollapsing;
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
        ImagesFromAsync(this DocumentRenderer dr, int page)
    {
        var ret = new List<IExtractedBitmap>();
        await dr.RenderPageToAsync(page, (rect, matrix) =>
        {
            var (width, height) = dr.ScalePageToRequestedSize(rect, new Vector2(-1, -1));
            var target = new ImageExtractorTarget(ret, page);
            dr.InitializeRenderTarget(target, rect, width, height, matrix);
            return target;
        }).CA();
        return ret;
    }

    /// <summary>
    /// Extract the Images from a given PDF page, and collapses images that are
    /// adjacent to one another.
    /// </summary>
    /// <param name="dr">DocumentRenderer than holds this document.</param>
    /// <param name="page">the one based page number to take images from</param>
    /// <returns>A list of images that appear on the page</returns>
    public static async ValueTask<IList<IExtractedBitmap>>
        CollapsedImagesFromAsync(this DocumentRenderer dr, int page) =>
        (await dr.ImagesFromAsync(page).CA()).CollapseAdjacentImages();



    /// <summary>
    /// Collapse bitmaps that are adjacent on the rendered page into a single image
    /// </summary>
    /// <param name="source">A list of IExtractedBitmaps</param>
    /// <returns>The passed parameter, with the images collapsed in place.</returns>
    public static IList<IExtractedBitmap> CollapseAdjacentImages(
        this IList<IExtractedBitmap> source)
    {
        new ImageColapser(source).Process();
        return source;
    }

    /// <summary>
    /// Extract the Images from a given PDF DocumentRenderer
    /// </summary>
    /// <param name="dr">DocumentRenderer than holds this document.</param>
    /// <returns>A list of images that appear on the page</returns>
    public static async IAsyncEnumerable<IExtractedBitmap>
        ImagesFromAsync(this DocumentRenderer dr)
    {
        for (int i = 0; i < dr.TotalPages; i++)
        {
            var images = await dr.ImagesFromAsync(i).CA();
            foreach (var image in images)
            {
                yield return image;
            }
        }
    }

    /// <summary>
    /// Extract the Images from a given PDF document, and collapses images that are
    /// adjacent to one another.
    /// </summary>
    /// <param name="dr">DocumentRenderer than holds this document.</param>
    /// <returns>A list of images that appear on the page</returns>
    public static async IAsyncEnumerable<IExtractedBitmap>
        CollapsedImagesFromAsync(this DocumentRenderer dr)
    {
        for (int i = 0; i<dr.TotalPages; i++)
        {
            var images = (await dr.ImagesFromAsync(i).CA()).CollapseAdjacentImages();
            foreach (var image in images)
            {
                yield return image;
            }
        }
    }

}