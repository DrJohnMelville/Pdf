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

        private void SetMode(RuleCollection rules, string mode)
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

        private void DeclareDependency(RuleCollection rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
            DeclareNegativeRule(rules, tokens.RightParam(), tokens.LeftParam());
        }

        private void DeclarePositiveDependency(RuleCollection rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
        }

        private void DeclareNegativeDependency(RuleCollection rules)
        {
            DeclareNegativeRule(rules, tokens.LeftParam(), tokens.RightParam());
        }

        private void DeclareExclusiveDependency(RuleCollection rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
            DeclareNegativeRule(rules, "*", tokens.RightParam());
        }

        private void DeclareEquivalence(RuleCollection rules)
        {
            DeclarePositiveRule(rules, tokens.LeftParam(), tokens.RightParam());
            DeclarePositiveRule(rules, tokens.RightParam(), tokens.LeftParam());
        }

        private void DeclarePositiveRule(RuleCollection rules, string use, string decl)
        {
            rules.DeclareRule(use, decl, "", true);
        }

        private void DeclareNegativeRule(RuleCollection rules, string use, string decl) 
            => rules.DeclareRule(use, decl, tokens.Text(), false);

        private void ParseGroup(RuleCollection rules)
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