using Melville.StreamInterfaces.Mixins;
using System;
using System.Runtime.CompilerServices;

namespace Melville.Parsing;

#if DEBUG
public static class InitializeModule
{
    /// <summary>
    /// Setup the memory leak detector
    /// </summary>
    [ModuleInitializer]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries", Justification = "<Pending>")]
    public static void SetupModule()
    {
        RequireDisposal.RecordDisposableCreation = false;
        RequireDisposal.ThrowOnMissedDispose = true;
    }
}
#endif