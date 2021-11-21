using System;
using Microsoft.CodeAnalysis;
using Pdf.KnownNamesGenerator.ContentStreamOperations;
using Pdf.KnownNamesGenerator.KnownNames;
using Pdf.KnownNamesGenerator.PostScriptOps;

namespace Pdf.KnownNamesGenerator
{
    [Generator]
    public class KnownNamesGeneratorClass: IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(Generate);
        }

        private void Generate(IncrementalGeneratorPostInitializationContext context)
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