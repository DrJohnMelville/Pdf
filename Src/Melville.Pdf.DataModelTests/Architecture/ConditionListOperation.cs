using FluentAssertions;
using NetArchTest.Rules;

namespace Melville.Pdf.DataModelTests.Architecture;

public static class ConditionListOperation
{
    public static void ShouldSucceed(this ConditionList cl) =>
        cl.GetResult().FailingTypeNames.Should().BeNull();
}