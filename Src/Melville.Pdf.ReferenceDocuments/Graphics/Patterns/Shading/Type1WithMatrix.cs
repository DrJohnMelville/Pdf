using System.Numerics;

namespace Melville.Pdf.ReferenceDocuments.Graphics.Patterns.Shading;

public class Type1WithMatrix : Type1FunctionalShaderBase
{
    public Type1WithMatrix() : base("Type 1 shader with pattern to domain matix")
    {
        
    }

    protected override DictionaryBuilder BuildShader(IPdfObjectCreatorRegistry arg, PdfStream[] localFunc,
        DictionaryBuilder builder) => 
        base.BuildShader(arg, localFunc, builder)
            .WithItem(KnownNames.MatrixTName, (
                Matrix3x2.CreateRotation(0.2f)
            ).AsPdfArray());
}