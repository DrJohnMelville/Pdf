using ArchitectureAnalyzer.Models;

namespace ArchitectureAnalyzer.Parsers
{
    public readonly struct RuleParser
    {
        private readonly RuleLexer tokens;

        public RuleParser(string input)
        {
            tokens = new RuleLexer(input);
        }
        public RuleCollection Parse()
        {
            var ret = new RuleCollection();
            while (!tokens.Done())
            {
                ParseToken(ret);
            }
            return ret;
        }

        private void ParseToken(RuleCollection rules)
        {
            switch (tokens.OpCode())
            {
                case "=>": 
                    DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
                    DeclareNegativeRule(rules, tokens.RightParam(), tokens.LeftParam());
                    break;
            }
            tokens.Next();
        }

        private void DeclarePositiveRule(RuleCollection rules, string use, string decl)
        {
            rules.DeclareRule(use, decl, "", true);
        }

        private void DeclareNegativeRule(RuleCollection rules, string use, string decl) 
            => rules.DeclareRule(use, decl, tokens.Text(), false);

    }
}