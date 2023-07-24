using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public class Type1WithDomain : Type1FunctionalShaderBase
{
    public Type1WithDomain() : base("Type 1 shader with a limitation to the domain")
    {
        
    }

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfValueStream[] localFunc,
        ValueDictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.DomainTName, new PdfValueArray(0.3,0.6, 0.25, 0.75));
}

public class Type1WithDomainAndBackground : Type1FunctionalShaderBase
{
    public Type1WithDomainAndBackground() : base("Type 1 shader with a limitation to the domain And a background")
    {
        
    }

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfValueStream[] localFunc,
        ValueDictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.DomainTName, new PdfValueArray(0.3,0.6, 0.25, 0.75))
            .WithItem(KnownNames.BackgroundTName, new PdfValueArray(1.0, 0,0));
}

public class Type1WithDomainBackgroundAndBBox : Type1FunctionalShaderBase
{
    public Type1WithDomainBackgroundAndBBox() : base("Type 1 shader with a limitation to the domain, a background, and a bounding box that overlap")
    {
        
    }

    protected override ValueDictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfValueStream[] localFunc,
        ValueDictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.DomainTName, new PdfValueArray(0.3, 0.6, 0.25, 0.75))
            .WithItem(KnownNames.BackgroundTName, new PdfValueArray(1.0, 0, 0))
            .WithItem(KnownNames.BBoxTName, new PdfValueArray(0.1, 0.4, 0.9, 0.6));
}