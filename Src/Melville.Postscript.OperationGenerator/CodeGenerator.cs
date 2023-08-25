﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Melville.INPC;
using Microsoft.CodeAnalysis;

namespace Melville.Postscript.OperationGenerator;

public readonly partial struct CodeGenerator
{
    [FromConstructor] private readonly ISymbol classSymbol;
    [FromConstructor] private readonly IEnumerable<GeneratorAttributeSyntaxContext> methods;
    private readonly StringBuilder output = new();


    public string CreateCode()
    {
        WritePrefix();
        foreach (var item in methods)
        {
            if (item.TargetSymbol is IMethodSymbol sym) CreateMethodClass(sym);
        }

        WriteAddOperationsMethod();
        WriteSuffix();
        return output.ToString();
    }

    private void WritePrefix()
    {
        output.AppendLine($$"""
                            // Implementation for PostscriptOperatorCollection names {{classSymbol.Name}}
                            // this file was auto-generated by Melville.Postscript.OperationGenerator
                            using Melville.Postscript.Interpreter.Values.Execution;
                            using Melville.Postscript.Interpreter.InterpreterState;
                            using Melville.Postscript.Interpreter.Values;
                            using Melville.Postscript.Interpreter.Values.Composites;
                            
                            namespace {{classSymbol.ContainingNamespace}};

                            internal static partial class {{classSymbol.Name}}
                            {
                            """);
    }

    private void WriteSuffix() => output.AppendLine("}");

    private void CreateMethodClass(IMethodSymbol item)
    {
        output.AppendLine($$"""
                                private sealed class {{item.Name}}BuiltInFunctionImpl: BuiltInFunction
                                {
                                    public override void Execute(PostscriptEngine engine, in PostscriptValue value)
                                    {
                            """);
        WriteMethodDelegation(item);
        output.AppendLine($$"""
                                    }
                                }
                            """);
    }

    private void WriteMethodDelegation(IMethodSymbol item)
    {
        CreateParameterVariables(item.Parameters);
        var (prefix, postfix) = item.ReturnsVoid ?
             ("            ", ";"):
             ("            engine.OperandStack.Push(", ");");
        output.Append(prefix);
        WriteMethodCall(item);
        output.AppendLine(postfix);
    }

    private void CreateParameterVariables(ImmutableArray<IParameterSymbol> parameters)
    {
        for (int i = parameters.Length - 1; i >= 0; i--)
        {
            CreateSingleParameterVariable(i, parameters[i]);
        }
    }

    private void CreateSingleParameterVariable(int i, IParameterSymbol parameter)
    {
        output.AppendLine($"            {parameter.Type} p{i} = {VarForType(parameter.Type)};");
    }

    private string VarForType(ITypeSymbol parameterType)
    {
        return parameterType.ToString() switch
        {
            "Melville.Postscript.Interpreter.InterpreterState.OperandStack"=> "engine.OperandStack",
            "Melville.Postscript.Interpreter.InterpreterState.ExecutionStack"=> "engine.ExecutionStack",
            "Melville.Postscript.Interpreter.InterpreterState.DictionaryStack"=> "engine.DictionaryStack",
            "Melville.Postscript.Interpreter.Values.PostscriptValue" => "engine.OperandStack.Pop()",
            "Melville.Postscript.Interpreter.InterpreterState.PostscriptEngine" => "engine",
            _=> $"engine.OperandStack.Pop().Get<{parameterType}>()"
        };
    }

    private void WriteMethodCall(IMethodSymbol item)
    {
        output.Append(item.Name);
        output.Append("(");
        output.Append(GenerateParameters(item));
        output.Append(")");
    }


    private void WriteAddOperationsMethod()
    {
        output.AppendLine("    public static void AddOperations(IPostscriptDictionary dictionary)");
        output.AppendLine("    {");
        foreach (var method in methods)
        {
            WriteOperatorDeclaration(method);
        }
        output.AppendLine("    }");
    }

    private void WriteOperatorDeclaration(GeneratorAttributeSyntaxContext method)
    {
        foreach (var methodAttribute in method.Attributes)
        {
            output.AppendLine(
                $"""        dictionary.Put("{PostscriptName(methodAttribute)}"u8, PostscriptValueFactory.Create(new {method.TargetSymbol.Name}BuiltInFunctionImpl()));""");
        }
    }

    private string GenerateParameters(ISymbol target) =>
        target is IMethodSymbol methodSym && methodSym.Parameters.Length is not 0
            ? string.Join(", ", Enumerable.Range(0, methodSym.Parameters.Length).Select(i => $"p{i}"))
            : "";

    private static string PostscriptName(AttributeData singleAttr)
    {
        return singleAttr.ConstructorArguments.First().Value?.ToString() ??
               throw new InvalidOperationException("Name is required.");
    }
}