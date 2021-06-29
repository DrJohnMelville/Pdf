using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArchitectureAnalyzer.Models
{
    public static class GlobRegexFactory
    {
        public static Regex CreateGlobRegex(string template) => 
            new(CreateGlobRegexString(template));

        private static string CreateGlobRegexString(string template) => 
            RequireWholeString(GenerateOperatorRecognizers(template));

        private static string RequireWholeString(string source) => $"^{source}$";

        private static string GenerateOperatorRecognizers(string template) =>
            Regex.Escape(template)
                .Replace("\\*",".*")
                .Replace("\\+",".+")
                .Replace("\\?",".");

        public static Regex CreateMultiGlobRecognizer(IEnumerable<string> options) => 
            new(string.Join("|", options.Select(CreateWrappedGlobRegexString)));

        private static  string CreateWrappedGlobRegexString(string i) => 
            $"(?:{CreateGlobRegexString(i)})";
    }
}