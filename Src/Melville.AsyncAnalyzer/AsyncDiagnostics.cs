using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Melville.AsyncAnalyzer;

public static class AsyncDiagnostics
{
    public static readonly DiagnosticDescriptor UnneededAsync = new(
        "Arch003", "Synchronous method has name ending with Async",
        "{0}", "Architecture", DiagnosticSeverity.Warning, true, "",
        "https://github.com/DrJohnMelville/Pdf/blob/main/Docs/Architecture/Achitecture.md",
        WellKnownDiagnosticTags.Build);
    public static readonly DiagnosticDescriptor AsyncNeeded = new(
        "Arch004", "Async method does not have name ending with Async",
        "{0}", "Architecture", DiagnosticSeverity.Warning, true, "",
        "https://github.com/DrJohnMelville/Pdf/blob/main/Docs/Architecture/Achitecture.md",
        WellKnownDiagnosticTags.Build);

    public static readonly ImmutableArray<DiagnosticDescriptor> All =
        ImmutableArray.Create(UnneededAsync, AsyncNeeded);
}