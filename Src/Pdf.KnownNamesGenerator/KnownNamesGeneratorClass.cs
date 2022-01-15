using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Pdf.KnownNamesGenerator.CharacterEncodings;
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
            context.RegisterSourceOutput(
                context.AdditionalTextsProvider.Where(i => i.Path.EndsWith("KnownNames.dsl")),
                GenerateNames);
            context.RegisterSourceOutput(context.AdditionalTextsProvider
                    .Where(i=>i.Path.EndsWith(".cedsl")).Collect(),
                GenerateCharacterEncodings);
        }

        private void GenerateCharacterEncodings(
            SourceProductionContext context, ImmutableArray<AdditionalText> additionalTexts)
        {
            context.AddSource("CharacterEncodings.Generated.cs", 
                EncodingGeneratorFactory.Create(additionalTexts).Generate());
        }

        private void GenerateNames(SourceProductionContext context, AdditionalText text)
        {
            context.AddSource("KnownNames.Generated.cs", 
               new GenerateKnownNames(text.GetText()?.ToString()??"").ClassText());
        }
        
        private void Generate(IncrementalGeneratorPostInitializationContext context)
        {
            context.AddSource("PostScriptOperations.Generated.cs",
                GeneratePostScriptOperations.ClassText());
            context.AddSource("ContentStreamOperations.Generated.cs",
                GenerateContentStreamOperations.ClassText());
        }
    }
}