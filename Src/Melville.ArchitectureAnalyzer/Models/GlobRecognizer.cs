using System.Text.RegularExpressions;

namespace ArchitectureAnalyzer.Models
{
    public readonly struct GlobRecognizer
    {
        private readonly Regex template;

        public GlobRecognizer(string template)
        {
            this.template = GlobRegexFactory.CreateGlobRegex(template);
        }

        public bool Matches(string item) => template.IsMatch(item);
    }
}