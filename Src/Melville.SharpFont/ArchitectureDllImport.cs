﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Melville.SharpFont
{
    /// <summary>
    /// Load the Freetype bibary for the selected architecture.
    /// </summary>
	public static class ArchitectureDllImport
	{
        /// <summary>
        /// Specify the folder for the FreeType binaries.
        /// </summary>
        /// <param name="explicitFolder">Full folder for the current architecture, or null to pick x87 or x64 binaries</param>
        public static void LoadArchitectureDependencyDirectory(string? explicitFolder)
        {
            var finalDllFolder = explicitFolder  ?? TryRootFolder() ?? InferredDllLocation();
            if (!string.IsNullOrWhiteSpace(finalDllFolder))
                SetDllDirectory(finalDllFolder);
        }

        private static string? TryRootFolder()
        {
            var ret = ThisAssemblyFolder();
            var fileName = Path.Combine(ret, "freetype6.dll");
            return (File.Exists(fileName)) ? ret : null;
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
            Path.Combine(ThisAssemblyFolder(), "runtimes", $"win-{dllPath}", "native");

        private static string ThisAssemblyFolder() =>
            Path.GetDirectoryName(typeof(ArchitectureDllImport).Assembly.Location) ??
            throw new InvalidOperationException("Cannot find folder for Melville.SharpFont");
        
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
