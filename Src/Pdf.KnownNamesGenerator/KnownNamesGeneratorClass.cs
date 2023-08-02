using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Pdf.KnownNamesGenerator.CharacterEncodings;
using Pdf.KnownNamesGenerator.KnownNames;

namespace Pdf.KnownNamesGenerator
{
    [Generator]
    public class KnownNamesGeneratorClass: IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(
                context.AdditionalTextsProvider.Where(i => i.Path.EndsWith("KnownNames.dsl")),
                GenerateNames);
            context.RegisterSourceOutput(context.AdditionalTextsProvider
                    .Where(i=>i.Path.EndsWith(".cedsl")).Collect(),
                GenerateCharacterEncodings);
            context.RegisterSourceOutput(context.AdditionalTextsProvider
                    .Where(i=>i.Path.EndsWith("glyphlist.cedsl")),
                GenerateGlyphList);
        }

        private void GenerateGlyphList(SourceProductionContext context, AdditionalText text)
        {
            context.AddSource("GlyphMapper.Generated.cs",
                new GlyphListWriter(GlyphNameParser.Parse(text.GetText()!.ToString())).Write());
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
        
    }
}