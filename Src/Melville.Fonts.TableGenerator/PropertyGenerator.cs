using System.Text;
using Microsoft.CodeAnalysis;

namespace Melville.Fonts.TableGenerator
{
    public sealed class PropertyGenerator(
        GeneratorAttributeSyntaxContext context, IPropertySymbol symbol) : MemberGenerator(context)
    {
        public override string Name => symbol.Name;
        protected override ITypeSymbol Type => symbol.Type;
        private static int varName = 0;
    
        protected override void InitializationLine(StringBuilder output)
        {
            var ord = $"transfer{++varName}";
            output.AppendLine($"""
                        global::Melville.Fonts.SfntParsers.TableParserParts.FieldParser.Read(ref reader, out {TypeName()} {ord});
                        this.{Name} = {ord};
                """);
        }
    }
}