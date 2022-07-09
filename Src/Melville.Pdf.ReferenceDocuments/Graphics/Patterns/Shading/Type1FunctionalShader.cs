using System.Collections.Immutable;
using System.Numerics;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public class Type1FunctionalShader : Type1FunctionalShaderBase
{
    public Type1FunctionalShader() : base("Type 1 shader from a pdffunction")
    {
        
    }
}

public class Type1WithDomain : Type1FunctionalShaderBase
{
    public Type1WithDomain() : base("Type 1 shader with a limitation to the domain")
    {
        
    }

    protected override DictionaryBuilder BuildShader(ILowLevelDocumentCreator arg, PdfObject[] localFunc, DictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.Domain, new PdfArray(0.3,0.6, 0.25, 0.75));
}
public class Type1WithMatrix : Type1FunctionalShaderBase
{
    public Type1WithMatrix() : base("Type 1 shader with pattern to domain matix")
    {
        
    }

    protected override DictionaryBuilder BuildShader(ILowLevelDocumentCreator arg, PdfObject[] localFunc, DictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.Matrix, (
                Matrix3x2.CreateRotation(0.2f)
            ).AsPdfArray());
}
public class Type1WithDomainAndBackground : Type1FunctionalShaderBase
{
    public Type1WithDomainAndBackground() : base("Type 1 shader with a limitation to the domain And a background")
    {
        
    }

    protected override DictionaryBuilder BuildShader(ILowLevelDocumentCreator arg, PdfObject[] localFunc, DictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.Domain, new PdfArray(0.3,0.6, 0.25, 0.75))
            .WithItem(KnownNames.Background, new PdfArray(1.0, 0,0));
}

public class Type1WithDomainBackgroundAndBBox : Type1FunctionalShaderBase
{
    public Type1WithDomainBackgroundAndBBox() : base("Type 1 shader with a limitation to the domain, a background, and a bounding box that overlap")
    {
        
    }

    protected override DictionaryBuilder BuildShader(ILowLevelDocumentCreator arg, PdfObject[] localFunc,
        DictionaryBuilder builder) =>
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.Domain, new PdfArray(0.3, 0.6, 0.25, 0.75))
            .WithItem(KnownNames.Background, new PdfArray(1.0, 0, 0))
            .WithItem(KnownNames.BBox, new PdfArray(0.1, 0.4, 0.9, 0.6));
}