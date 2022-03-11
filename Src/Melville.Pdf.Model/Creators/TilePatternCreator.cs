using System;
using System.Linq;
using System.Numerics;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamDataSources;
using Melville.Pdf.LowLevel.Model.Wrappers;
using Melville.Pdf.LowLevel.Writers;
using Melville.Pdf.LowLevel.Writers.Builder;

namespace Melville.Pdf.Model.Creators;

public enum PatternPaintType
{
    Colored = 1,
    Uncolored = 2
}

public enum PatternTileType 
{
   Constant = 1,
   NoDistortion = 2,
   FastConstant = 3
}

public class TilePatternCreator : ContentStreamCreator
{
    private MultiBufferStream? content;
    
    public TilePatternCreator(PatternPaintType paint, PatternTileType tile,
        double xStep, double yStep, PdfRect bBox, IObjectStreamCreationStrategy objStreamStrategy) 
        : base(objStreamStrategy)
    {
        MetaData.WithItem(KnownNames.Type, KnownNames.Pattern)
         .WithItem(KnownNames.PatternType, 1)   
         .WithItem(KnownNames.PaintType,  (int)paint)
         .WithItem(KnownNames.TilingType, (int)tile)
         .WithItem(KnownNames.XStep, xStep)
         .WithItem(KnownNames.YStep, yStep)
         .WithItem(KnownNames.BBox, bBox.ToPdfArray)
         .WithItem(KnownNames.PatternType, 1);
    }

    public void AddMatrix(Matrix3x2 matrix) => MetaData.WithItem(KnownNames.Matrix, matrix.AsPdfArray());

    public override (PdfIndirectReference Reference, int PageCount) ConstructPageTree(
        ILowLevelDocumentCreator creator, PdfIndirectReference? parent, int maxNodeSize)
    {
        if (parent != null)
            throw new InvalidOperationException("Patterns may not have a parent");
        return base.ConstructPageTree(creator, parent, maxNodeSize);
    }

    public override void AddToContentStream(DictionaryBuilder builder, MultiBufferStreamSource data)
    {
        if (content != null) 
            throw new InvalidOperationException("Tile Pattern may have only 1 content stream");
        content = data.Stream;
    }

    protected override PdfIndirectReference CreateFinalObject(ILowLevelDocumentCreator creator)
    {
        if (content is null) throw new InvalidOperationException("Tile Pattern must have content.");
        return creator.Add(MetaData.AsStream(content));
    }
}