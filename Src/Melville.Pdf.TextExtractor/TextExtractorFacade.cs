﻿using Melville.Pdf.Model.Renderers.DocumentRenderers;

namespace Melville.Pdf.TextExtractor;

/// <summary>
/// This class allows text to be extracted from a PDF page or file
/// </summary>
public static class TextExtractorFacade
{
    /// <summary>
    /// Gets the text that shows up on a given page of a given Pdf file
    /// </summary>
    /// <param name="renderer">DocumentRenderer for the file to be read</param>
    /// <param name="oneBasedPageNumber">One based page number of the desired page.</param>
    /// <returns></returns>
    public static async ValueTask<string> PageTextAsync(
        this DocumentRenderer renderer, int oneBasedPageNumber) =>
        await RenderTextAsync(renderer, oneBasedPageNumber, new ConcatenateAndCollapseHyphenTarget());

    /// <summary>
    /// Gets the text that shows up on a given page of a given Pdf file.
    /// This overload leaves terminal hyphens intact.
    /// </summary>
    /// <param name="renderer">DocumentRenderer for the file to be read</param>
    /// <param name="oneBasedPageNumber">One based page number of the desired page.</param>
    /// <returns></returns>
    public static async ValueTask<string> PageTextWithTerminalHyphensAsync(
        this DocumentRenderer renderer, int oneBasedPageNumber) =>
        await RenderTextAsync(renderer, oneBasedPageNumber, new ConcatenatingTextTarget());

    private static async Task<string> RenderTextAsync(
        DocumentRenderer renderer, int oneBasedPageNumber, ConcatenatingTextTarget target)
    {
        await renderer.RenderPageToAsync(oneBasedPageNumber,
            (_,__)=>new ExtractTextRender(target));
        return target.AllText();
    }
}