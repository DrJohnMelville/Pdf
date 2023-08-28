using System.Text;
using Melville.INPC;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.OperationGenerator;

internal abstract class MethodWriter
{
    private static readonly MethodWriter Void = new VoidMethodWriter();
    private static readonly MethodWriter SyncReturn = new ReturnMethodWriter();
    private static readonly MethodWriter AsyncVoid = new AsyncVoidMethodWriter();
    private static readonly MethodWriter AsyncReturn = new AsyncReturnMethodWriter();

    public abstract void WriteObjectPrefix(StringBuilder output, IMethodSymbol symbol);
    public abstract string CallPrefix();
    public abstract string CallPostFix();

    public static MethodWriter Classify(IMethodSymbol symbol) =>
        symbol.ReturnsVoid ? Void : ClassifyReturnType(symbol.ReturnType.ToString());

    private static MethodWriter ClassifyReturnType(string typeName) => typeName switch
    {
        _ when typeName.Contains("ValueTask<") => AsyncReturn,
        _ when typeName.Contains("ValueTask") => AsyncVoid,
        _ => SyncReturn
    };

    private class VoidMethodWriter : MethodWriter
    {

        public override void WriteObjectPrefix(StringBuilder output, IMethodSymbol item) =>
            output.AppendLine(
                $$"""
                      private sealed class {{item.Name}}BuiltInFunctionImpl: BuiltInFunction
                      {
                          public override void Execute(PostscriptEngine engine, in PostscriptValue value)
                          {
                  """);

        public override string CallPrefix() => "";

        public override string CallPostFix() => ";";
    }

    private class ReturnMethodWriter : VoidMethodWriter
    {
        public override string CallPrefix() => "engine.OperandStack.Push(";

        public override string CallPostFix() => ");";
    }

    private class AsyncVoidMethodWriter : MethodWriter
    {

        public override void WriteObjectPrefix(StringBuilder output, IMethodSymbol item) =>
            output.AppendLine(
                $$"""
                      private sealed class {{item.Name}}BuiltInFunctionImpl: AsyncBuiltInFunction
                      {
                          public override ValueTask ExecuteAsync(PostscriptEngine engine, in PostscriptValue value)
                          {
                  """);

        public override string CallPrefix() => "return ";

        public override string CallPostFix() => ";";
    }
    private class AsyncReturnMethodWriter : AsyncVoidMethodWriter
    {
        public override void WriteObjectPrefix(StringBuilder output, IMethodSymbol item)
        {
            base.WriteObjectPrefix(output, item);
            output.AppendLine("""
                           return InnerItem(engine);
                       }
                       
                       private async ValueTask InnerItem(PostscriptEngine engine)
                       {    
               """);
        }

        public override string CallPrefix() => "engine.OperandStack.Push(await ";

        public override string CallPostFix() => ".CA());";
    }

}