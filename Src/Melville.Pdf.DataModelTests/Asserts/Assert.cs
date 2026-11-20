using Melville.Icc.Model;
using Melville.INPC;
using Melville.Parsing.ObjectRentals;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melville.Pdf.DataModelTests.Asserts;

public static partial class Assert
{
    [MacroItem("int")]
    [MacroItem("uint")]
    [MacroItem("bool")]
    [MacroItem("bool?")]
    [MacroItem("float")]
    [MacroItem("string")]
    [MacroItem("PdfTime", typeof(PdfTime))]
    [MacroCode("public static void Equal(~0~ expected, ~0~ seen) => seen.Should().Be(expected);")]
    static partial void EqualMethods();

    public static void True(bool b) => b.Should().BeTrue();
    public static void True(bool? b) => b.Should().BeTrue();
    public static void False(bool b) => b.Should().BeFalse();

    public static void Equal(double expected, double seen, int digits, MidpointRounding _)
        => Equal(expected, seen, digits);
    public static void Equal(double expected, double seen, int digits) =>
        seen.Should().BeApproximately(expected, (1 / Math.Pow(10, digits - 1)));
    public static void Equal<T>(T expected, T seen) => seen.Should().Be(expected);
    public static void SeqEqual<T>(IEnumerable<T> expected, IEnumerable<T> seen) => seen.Should().BeEquivalentTo(expected);
    public static void Same<T>(T expected, T seen) => seen.Should().BeSameAs(expected);

    public static void Contains(string expected, string value) => value.Should().Contain(expected);
    public static void StartsWith(string expected, string value) => value.Should().StartWith(expected);
    public static void DoesNotContain(string expected, string value) => value.Should().NotContain(expected);

    public static void Empty<T>(IEnumerable<T> item) => item.Should().BeEmpty();
    public static void Single<T>(IEnumerable<T> item) => item.Should().HaveCount(1);
    public static void IsType<T>(object item) => item.Should().BeOfType<T>();
    public static void NotNull(object? item) => item.Should().NotBeNull();

    public static void Throws<T>(Action act) where T:Exception => 
        1.Invoking(_ => act()).Should().Throw<T>();

    public static Task ThrowsAsync<T>(Func<Task> act) where T : Exception =>
        1.Invoking(_ => act()).Should().ThrowAsync<T>();
}