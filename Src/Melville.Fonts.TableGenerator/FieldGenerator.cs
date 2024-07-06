using System.Text;
using Microsoft.CodeAnalysis;

namespace Melville.Fonts.TableGenerator
{
    public sealed class FieldGenerator(
        GeneratorAttributeSyntaxContext context, IFieldSymbol symbol) : MemberGenerator(context)
    {
        public override string Name => symbol.Name;
        protected override ITypeSymbol Type => symbol.Type;
    
        protected override void InitializationLine(StringBuilder output) =>
            output.AppendLine(
                $"        global::Melville.Fonts.SfntParsers.TableParserParts.FieldParser.Read(ref reader, out this.{Name});");
    }
}