using System.Text.RegularExpressions;

namespace ArchitectureAnalyzer.Models
{
    public static class GlobRegexFactory
    {
        public static Regex CreateGlobRegex(string template) => 
            new Regex(RewuireWholeString(GenerateOperatorRecognizers(template)));

        private static string RewuireWholeString(string source) => $"^{source}$";

        private static string GenerateOperatorRecognizers(string template) =>
            Regex.Escape(template)
                .Replace("\\*",".*")
                .Replace("\\+",".+")
                .Replace("\\?",".");
    }
}