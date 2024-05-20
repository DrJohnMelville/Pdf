using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Melville.Fonts.TableGenerator;

public abstract class MemberGenerator(GeneratorAttributeSyntaxContext context)
{
    public void ConstructorLine(StringBuilder output)
    {
        if (IsArray)
            output.AppendLine($"        this.{Name} = new {ArrayElementType(Type)}[{SizeCode()}];");
        else
            InitializationLine(output);
    }

    protected abstract void InitializationLine(StringBuilder output);

    private string ArrayElementType(ITypeSymbol type) => type switch
        {
            IArrayTypeSymbol at => ArrayElementType(at.ElementType),
            _=> TryAddGlobal(type.ToString())
         };

    private string TryAddGlobal(string s) => s.Contains('.') ? $"global::{s}" : s;

    public void LoadArrayLine(StringBuilder output, string prefix, string postfix)
    {
        Debug.Assert(IsArray);
        output.AppendLine($"""
                    {prefix} global::Melville.Fonts.SfntParsers.TableParserParts.FieldParser.ReadAsync(
                        reader, this.{Name}){postfix};
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