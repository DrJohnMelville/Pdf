using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace ArchitectureAnalyzer;

public static class DocumentVisibleMembersDiagnostics
{
    public static readonly DiagnosticDescriptor RuleViolated = new(
        "Arch003", "Externaly visible member much have xml doc comment.",
        "{0}", "Architecture", DiagnosticSeverity.Warning, true,
        "This member can be seen outside of this module and thus requires xml documentation",
        "https://github.com/DrJohnMelville/Pdf/blob/main/Docs/Architecture/Achitecture.md",
        WellKnownDiagnosticTags.AnalyzerException);

    public static readonly ImmutableArray<DiagnosticDescriptor> All =
        ImmutableArray.Create(
            RuleViolated);

}