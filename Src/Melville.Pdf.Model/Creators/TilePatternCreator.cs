using System;
using System.Numerics;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

/// <summary>
/// Pdf offers two type to TilePatterns this distinguished between them
/// </summary>
public enum PatternPaintType
{
    /// <summary>
    /// Specifies a colored tile pattern
    /// </summary>
    Colored = 1,

    /// <summary>
    /// Specifies an uncolored tile pattern
    /// </summary>
    Uncolored = 2
}

/// <summary>
/// Pdf offers 3 types of tile pattern tiling modes.
/// </summary>
public enum PatternTileType
{
    /// <summary>
    /// Allow small distortions of less that 1 device pixel to align tiles to device pixels
    /// </summary>
    Constant = 1,

    /// <summary>
    /// Space consistently in PDF space with no distortion
    /// </summary>
    NoDistortion = 2,

    /// <summary>
    /// Allow more than one device pixel distortion to ensure faster tiling
    /// </summary>
    FastConstant = 3
}

/// <summary>
/// This is a builder class used to make tile pattern dictionaries
/// </summary>
public class TilePatternCreator : ContentStreamCreator
{
    private MultiBufferStream? content;

    /// <summary>
    /// Create a TilePatternCreator
    /// </summary>
    /// <param name="paint">Determines if this is a colored or uncolored tile pattern</param>
    /// <param name="tile">Sets the tiling type for this tile pattern</param>
    /// <param name="xStep">Horizontal spacing between pattern cells, in pattern units.</param>
    /// <param name="yStep">Vertical spacing between pattern cells, in pattern units.</param>
    /// <param name="bBox">Bounding box of the pattern cell in pattern units</param>
    public TilePatternCreator(PatternPaintType paint, PatternTileType tile,
        double xStep, double yStep, PdfRect bBox)
        : base(NoObjectStream.Instance)
    {
        MetaData.WithItem(KnownNames.Type, KnownNames.Pattern)
            .WithItem(KnownNames.PatternType, 1)
            .WithItem(KnownNames.PaintType, (int)paint)
            .WithItem(KnownNames.TilingType, (int)tile)
            .WithItem(KnownNames.XStep, xStep)
            .WithItem(KnownNames.YStep, yStep)
            .WithItem(KnownNames.BBox, bBox.ToPdfArray);
    }

    /// <summary>
    /// Add a matrix to the pattern dictionay
    /// </summary>
    /// <param name="matrix"></param>
    public void AddMatrix(Matrix3x2 matrix) => MetaData.WithItem(KnownNames.Matrix, matrix.AsPdfArray());

    /// <inheritdoc />
    public override (PdfIndirectObject Reference, int PageCount) ConstructItem(IPdfObjectCreatorRegistry creator,
        PdfIndirectObject parent)
    {
        if (!parent.IsNull)
            throw new InvalidOperationException("Patterns may not have a parent");
        return base.ConstructItem(creator, parent);
    }

    /// <inheritdoc />
    public override void AddToContentStream(DictionaryBuilder builder, MultiBufferStreamSource data)
    {
        if (content != null)
            throw new InvalidOperationException("Tile Pattern may have only 1 content stream");
        content = data.Stream;
    }

    /// <inheritdoc />
    protected override PdfIndirectObject CreateFinalObject(IPdfObjectCreatorRegistry creator)
    {
        if (content is null) throw new InvalidOperationException("Tile Pattern must have content.");
        return creator.Add(MetaData.AsStream(content));
    }
}