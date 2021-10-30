using System;
using Microsoft.CodeAnalysis;
using Pdf.KnownNamesGenerator.ContentStreamOperations;
using Pdf.KnownNamesGenerator.KnownNames;
using Pdf.KnownNamesGenerator.PostScriptOps;

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
            context.AddSource("PostScriptOperations.Generated.cs",
                GeneratePostScriptOperations.ClassText());
            context.AddSource("ContentStreamOperations.Generated.cs",
                GenerateContentStreamOperations.ClassText());
        }
    }
}