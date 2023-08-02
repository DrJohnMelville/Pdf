namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public class Type1WithDomain : Type1FunctionalShaderBase
{
    public Type1WithDomain() : base("Type 1 shader with a limitation to the domain")
    {
        
    }

    protected override DictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfStream[] localFunc,
        DictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.DomainTName, new PdfArray(0.3,0.6, 0.25, 0.75));
}

public class Type1WithDomainAndBackground : Type1FunctionalShaderBase
{
    public Type1WithDomainAndBackground() : base("Type 1 shader with a limitation to the domain And a background")
    {
        
    }

    protected override DictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfStream[] localFunc,
        DictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.DomainTName, new PdfArray(0.3,0.6, 0.25, 0.75))
            .WithItem(KnownNames.BackgroundTName, new PdfArray(1.0, 0,0));
}

public class Type1WithDomainBackgroundAndBBox : Type1FunctionalShaderBase
{
    public Type1WithDomainBackgroundAndBBox() : base("Type 1 shader with a limitation to the domain, a background, and a bounding box that overlap")
    {
        
    }

    protected override DictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfStream[] localFunc,
        DictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.DomainTName, new PdfArray(0.3, 0.6, 0.25, 0.75))
            .WithItem(KnownNames.BackgroundTName, new PdfArray(1.0, 0, 0))
            .WithItem(KnownNames.BBoxTName, new PdfArray(0.1, 0.4, 0.9, 0.6));
}