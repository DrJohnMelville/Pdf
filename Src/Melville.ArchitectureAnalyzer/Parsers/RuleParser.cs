using System;
using System.Collections.Generic;
using System.Threading;
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
        public DependencyRules Parse()
        {
            var ret = new DependencyRules();
            while (!tokens.Done())
            {
                ParseToken(ret);
            }
            return ret;
        }
        
        private void ParseToken(DependencyRules rules)
        {
            switch (tokens.OpCode())
            {
                case "#": break; // ignore comments
                case "=>": 
                    DeclareDependency(rules);
                    break;
                case "+=>":
                    DeclarePositiveDependency(rules);
                    break;
                case "!=>":
                    DeclareNegativeDependency(rules);
                    break;
                case "^=>":
                    DeclareExclusiveDependency(rules);
                    break;
                case "<=>":
                    DeclareEquivalence(rules);
                    break;
                case "Group":
                    ParseGroup(rules);
                    return;
                case "Mode":
                    SetMode(rules, tokens.LeftParam());
                    break;
                case var s when IsMemberOpcode(s):
                    throw new DslException($"Group member \"{tokens.LeftParam()}\" must follow a group definition.");
                default:
                    throw new DslException($"Unknown opcode \"{tokens.OpCode()}\"");
            }
            tokens.Next();
        }

        private void SetMode(DependencyRules rules, string mode)
        {
            switch (mode)
            {
                case "Strict": 
                    rules.SetStrictMode();
                    break;
                case "Loose": 
                    rules.SetLooseMode();
                    break;
            }
        }

        private void DeclareDependency(DependencyRules rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
            DeclareNegativeRule(rules, tokens.RightParam(), tokens.LeftParam());
        }

        private void DeclarePositiveDependency(DependencyRules rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
        }

        private void DeclareNegativeDependency(DependencyRules rules)
        {
            DeclareNegativeRule(rules, tokens.LeftParam(), tokens.RightParam());
        }

        private void DeclareExclusiveDependency(DependencyRules rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
            DeclareNegativeRule(rules, "*", tokens.RightParam());
        }

        private void DeclareEquivalence(DependencyRules rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
            DeclarePositiveRule(rules, tokens.RightParam(), tokens.LeftParam());
        }

        private void DeclarePositiveRule(DependencyRules rules, string use, string decl)
        {
            rules.DeclareRule(use, decl, "", true);
        }

        private void DeclareNegativeRule(DependencyRules rules, string use, string decl) 
            => rules.DeclareRule(use, decl, tokens.Text(), false);

        private void ParseGroup(DependencyRules rules)
        {
            var groupName = tokens.LeftParam();
            var groupMembers = new List<string>();
            while (tokens.Next() && IsMemberOpcode(tokens.OpCode()))
            {
                groupMembers.Add(tokens.LeftParam());           
            }
            rules.DefineGroup(groupName, groupMembers);
            rules.DeclareRule(groupName, groupName, "", true);
        }
        private bool IsMemberOpcode(string opCode) => opCode.Length > 0 && char.IsWhiteSpace(opCode[0]);
    }

    public class DslException : Exception
    {
        public DslException(string message) : base(message)
        {
        }
    }
}