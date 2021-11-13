using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArchitectureAnalyzer.Parsers;

public class RuleLexer
{
    private readonly string input;
    private IList<Match> Tokens { get; }
    private int position;

    public RuleLexer(string input)
    {
        this.input = input;
        Tokens = tokenFinder.Matches(input).OfType<Match>().ToList();
        EnsurePrefixIsWhitespace();
    }

    private Regex tokenFinder = new(@"
^(?'Left'\!*[\w\.\+\*\?]+)\s*(?'Op'[\!\^\<\+]?=>)\s*(?'Right'\!*[\w\.\+\?\*]+)| #dependency
^(?'Op'Group)\s+(?'Left'\w+)|  #Group Declaration
^(?'Op'[\ \t]+)(?'Left'\!*[\w\.\+\*\?]+)| #Group member declaration
^(?'Op'Mode)\s+(?'Left'Loose|Strict) |
(?'Op'\#).*$ # comment
", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

    public Match Current => Tokens[position];
    public string OpCode() => Current.Groups["Op"].Value;
    public string LeftParam() => Current.Groups["Left"].Value;
    public string RightParam() => Current.Groups["Right"].Value;
    public string Text() => Current.Value;

    public bool Next()
    {
        // throws if position >= length
        position++;
        EnsureWhitespace(PriorTokenEnd(),CurrentStartOrEndOfInput());
        return !Done();
    }

    private int CurrentStartOrEndOfInput() => 
        position >= Tokens.Count ? input.Length : Tokens[position].Index;

    private int PriorTokenEnd()
    {
        var priorToken = Tokens[position-1];
        return priorToken.Index+priorToken.Length;
    }

    public bool Done() => position >= Tokens.Count();

    private void EnsurePrefixIsWhitespace()
    {
        EnsureWhitespace(0, GetPrefixLength());
    }
    private int GetPrefixLength() => Tokens.Count > 0 ? Tokens[0].Index : input.Length;

    private void EnsureWhitespace(int origin, int exclusiveEndpoint)
    {
        for (int i = origin; i < exclusiveEndpoint; i++)
        {
            if (!char.IsWhiteSpace(input[i]))
            {
                throw new DslException(
                    $"\"{SubstringForErrorMessage(origin, exclusiveEndpoint)}\" is not a rule.");
            }
        }
    }

    private string SubstringForErrorMessage(int origin, int exclusiveEndpoint) => 
        input.Substring(origin,exclusiveEndpoint - origin).Trim();
}