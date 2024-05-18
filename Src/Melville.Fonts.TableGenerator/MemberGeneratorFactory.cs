using System.IO;
using Microsoft.CodeAnalysis;

namespace Melville.Fonts.TableGenerator
{
    public static class MemberGeneratorFactory
    {
        public static MemberGenerator Create(GeneratorAttributeSyntaxContext ctx) =>
            ctx.TargetSymbol switch
            {
                IFieldSymbol fs => new FieldGenerator(ctx, fs),
                IPropertySymbol fs => new PropertyGenerator(ctx, fs),
                var x => throw new InvalidDataException($"Cannot generate field: {x}")
            };
    }
}