using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Melville.SharpFont
{
	public static class ArchitectureDllImport
	{
        public static void LoadArchitectureDependencyDirectory(string? explicitFolder)
        {
            var finalDllFolder = explicitFolder ?? InferredDllLocation();
            if (!string.IsNullOrWhiteSpace(finalDllFolder))
                SetDllDirectory(finalDllFolder);
        }
        private static string InferredDllLocation() => RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => PathForArchitecture("x86"),
            Architecture.X64 => PathForArchitecture("x64"),
            var arch => throw new ArgumentOutOfRangeException(
                $"Using the {arch} architecture requires the user to explicitly set the " +
                $"Freetype6.dll folder by calling GlobalFreetypeResources.LoadFontLibrary in Melville.PDF " +
                $"or passing a string to the Library constructor in Melville.SharpFont")
        };

        private static string PathForArchitecture(string dllPath) => 
            Path.Combine(ThisAssemblyFolder(), "lib", dllPath);

        private static string ThisAssemblyFolder() =>
            Path.GetDirectoryName(typeof(ArchitectureDllImport).Assembly.Location) ??
            throw new InvalidOperationException("Cannot find folder for Melville.SharpFont");
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
