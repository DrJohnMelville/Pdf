using System;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using Melville.INPC;

namespace Melville.Pdf.DataModelTests.SpanShould;

public static class SpanShouldsOperations
{
    public static SpanShoulds<T> Should<T>(this Span<T> span) =>
        new(span);

    public static SpanShoulds<T> Should<T>(this ReadOnlySpan<T> span) =>
        new(span);
}

public ref partial struct SpanShoulds<T>
{
    [FromConstructor] public ReadOnlySpan<T> Subject { get; }

    public void Be(
        ReadOnlySpan<T> expected, string because = "", params object[] becauseArgs) =>
        Execute.Assertion
            .ForCondition(expected.SequenceEqual(Subject))
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected spans to be equivalent to {0}, but it was not.\r\n{1}\r\n{2}", 
                because, PrintSpan(expected), PrintSpan(Subject));

    private string PrintSpan(ReadOnlySpan<T> expected)
    {
        var ret = new StringBuilder();
        ret.Append("{");
        if (expected.Length > 0)
            ret.Append(expected[0]);
        for (int i = 1; i < expected.Length; i++)
        {
            ret.Append(", ");
            ret.Append(expected[i]);
        }
        ret.Append("}");
        return ret.ToString();
    }
}