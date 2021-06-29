using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArchitectureAnalyzer.Parsers
{
    public class RuleLexer
    {
        private IList<Match> Tokens { get; }
        private int position;

        public RuleLexer(string input)
        {
            Tokens = tokenFinder.Matches(input).OfType<Match>().ToList();
        }

        private Regex tokenFinder = new(@"
^(?'Left'[\w\.\+\*\?]+)\s*(?'Op'[\!\^\<\+]?=>)\s*(?'Right'[\w\.\+\?\*]+)| #dependency
^(?'Op'Group)\s+(?'Left'\w+)|  #Group Declaration
^(?'Op'\s+)(?'Left'[\w\.\+\*\?]+)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

        public Match Current => Tokens[position];
        public string OpCode() => Current.Groups["Op"].Value;
        public string LeftParam() => Current.Groups["Left"].Value;
        public string RightParam() => Current.Groups["Right"].Value;
        public string Text() => Current.Value;

        public bool Next() => ++position < Tokens.Count;
        public bool Done() => position >= Tokens.Count();
    }
}