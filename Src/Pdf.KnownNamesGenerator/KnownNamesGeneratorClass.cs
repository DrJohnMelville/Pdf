using System;
using Microsoft.CodeAnalysis;

namespace Pdf.KnownNamesGenerator
{
    [Generator]
    public class KnownNamesGeneratorClass: ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // do nothing
        }

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("KnownNames.Generated.cs",
                GenerateKnownNames.ClassText());
        }
    }
}