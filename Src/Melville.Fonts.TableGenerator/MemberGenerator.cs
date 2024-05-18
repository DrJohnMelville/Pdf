using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Melville.Fonts.TableGenerator;

public static class MemberGeneratorFactory
{
    public static MemberGenerator Create(GeneratorAttributeSyntaxContext ctx) =>
        ctx.TargetSymbol switch
        {
            IFieldSymbol fs => new FieldGenerator(ctx, fs),
            var x => throw new InvalidDataException($"Cannot generate field: {x}")
        };

    public static IEnumerable<T> SideEffect<T>(this IEnumerable<T> col, Action<T> op)
    {
        foreach (var item in col)
        {
            op(item);
            yield return item;
        }
    }
}

public abstract class MemberGenerator(GeneratorAttributeSyntaxContext context)
{
    public void ConstructorLine(StringBuilder output)
    {
        if (IsArray)
            output.AppendLine($"        this.{Name} = new {ArrayElementType(Type)}[{SizeCode()}];");
        else
            output.AppendLine(
            $"        global::Melville.Fonts.SfntParsers.TableParserParts.FieldParser.Read(ref reader, out this.{Name});");
    }
    
    private string ArrayElementType(ITypeSymbol type) => type switch
        {
            IArrayTypeSymbol at => ArrayElementType(at.ElementType),
            _=> TryAddGlobal(type.ToString())
         };

    private string TryAddGlobal(string s) => s.Contains('.') ? $"global:{s}" : s;

    public void LoadArrayLine(StringBuilder output)
    {
        if (!IsArray) return;
        output.AppendLine($"""
                    await global::Melville.Fonts.SfntParsers.TableParserParts.FieldParser.ReadAsync(
                        reader, this.{Name}).CA();
            """);
    }

    public abstract string Name { get; }
    protected abstract ITypeSymbol Type { get; }
    public string TypeName() => TryAddGlobal(Type.ToString());
    public bool IsArray => Type is IArrayTypeSymbol;
    
    private string SizeCode()
    {
        return context.Attributes
            .Where(i => i.AttributeClass?.Name.Equals("SFntFieldAttribute", StringComparison.Ordinal) ?? false)
            .SelectMany(i => i.ConstructorArguments.Concat(i.NamedArguments.Select(j=>j.Value)))
            .Select(i=>i.Value?.ToString() ?? "Null Value length param")
            .DefaultIfEmpty("Arrays need a size parameter on the attribute")
            .First();
    }
}

public sealed class FieldGenerator(
    GeneratorAttributeSyntaxContext context, IFieldSymbol symbol) : MemberGenerator(context)
{
    public override string Name => symbol.Name;
    protected override ITypeSymbol Type => symbol.Type;
}