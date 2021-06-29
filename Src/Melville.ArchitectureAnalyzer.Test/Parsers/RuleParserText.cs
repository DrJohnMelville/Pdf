using System;
using System.Runtime.InteropServices;
using ArchitectureAnalyzer.Models;
using ArchitectureAnalyzer.Parsers;
using Xunit;

namespace Melville.ArchitectureAnalyzer.Test.Parsers
{
    public class RuleParserText
    {
        private static DependencyRules ParseInput(string input) => new RuleParser(input).Parse();

        [Theory]
        [InlineData("")]
        [InlineData("Mode Loose\r\nMode Strict")]
        public void DefaultsToStrictMode(string text)
        {
            var rules = ParseInput(text);
            Assert.Equal("\"A\" may not reference \"B\" because \"No dependency rule found\"", 
                rules.ErrorFromReference("A","B"));
        }
        [Fact]
        public void LooseModeAllowsUnSpecifiedDependencies()
        {
            var rules = ParseInput("Mode Loose");
            Assert.Null(rules.ErrorFromReference("A","B"));
        }

        [Theory]
        [InlineData("A.B.Class","C.D.E", null)] // dependence is allowed
        [InlineData("C.D.E","A.B.Class", // Inverse dependency disallowed
            "\"C.D.E\" may not reference \"A.B.Class\" because \"A.B.* => C.D.?\"")]
        // check that * and ? properly exclude inelligible classes
        [InlineData("A.C.B", "C.D.E", "\"A.C.B\" may not reference \"C.D.E\" because \"No dependency rule found\"")]
        [InlineData("A.B.B", "C.D.EF", "\"A.B.B\" may not reference \"C.D.EF\" because \"No dependency rule found\"")]
        
        public void ParseDependency(string use, string decl, string? error)
        {
            var rules = ParseInput("A.B.* => C.D.?");
            Assert.Equal(error, rules.ErrorFromReference(use, decl));
            
        }

        [Fact]
        public void ForbidDependency()
        {
            var rules = ParseInput("A* !=> B*");
            Assert.Equal("\"A\" may not reference \"B\" because \"A* !=> B*\"", 
                rules.ErrorFromReference("A","B"));
        }
        [Fact]
        public void AllowDependency()
        {
            var rules = ParseInput("A* +=> B*");
            Assert.Null(rules.ErrorFromReference("A","B"));
        }

        [Fact]
        public void ExclusiveRule()
        {
            var rules = ParseInput("C* ^=> B*");
            Assert.Null(rules.ErrorFromReference("Charlie", "Bravo"));
            Assert.Equal("\"A\" may not reference \"B\" because \"C* ^=> B*\"", 
                rules.ErrorFromReference("A","B"));
        }

        [Fact]
        public void EquivalenceClass()
        {
            var rules = ParseInput("C* <=> B*");
            Assert.Null(rules.ErrorFromReference("Charlie", "Bravo"));
            Assert.Null(rules.ErrorFromReference("Bravo", "Charlie"));
        }
        
        [Theory]
        [InlineData("Alpha", "Uniform")] // members of group can access dependencies
        [InlineData("Echo", "Uniform")]
        [InlineData("Igloo", "Uniform")]
        [InlineData("Igloo", "Alpha")] // members of group can access each other.
        [InlineData("Alpha", "Alpha")] 
        [InlineData("Alpha", "AlRex")] 
        [InlineData("Echo", "AlRex")] 
        public void DefineLayer(string use, string decl)
        {
            var rules = ParseInput("Group Vowe1s\r\n  A*\r\n  E*\r\n  I*\r\nVowels => U*");
            Assert.Null(rules.ErrorFromReference(use, decl));
        }

        [Fact]
        public void DuplicateGroupDeclIsInvald()
        {
            Assert.Throws<DslException>(() => ParseInput("Group XXX\r\n  A\r\nGroup XXX"));
        }

        [Theory]
        [InlineData("Foo", "\"Foo\" is not a rule.")]
        [InlineData("Foo\r\na=>b", "\"Foo\" is not a rule.")]
        [InlineData("a=>b\r\nFoo", "\"Foo\" is not a rule.")]
        [InlineData("a=>b\r\nFoo\r\nGroup XXYY", "\"Foo\" is not a rule.")]
        [InlineData("  Foo", "Group member \"Foo\" must follow a group definition.")]
        public void ParserError(string input, string message)
        {
            try
            {
                ParseInput(input);
            }
            catch (DslException e)
            {
                Assert.Equal(message, e.Message);
                return;
            }
            Assert.False(true, "Failed to throw exception");
        }

        [Theory]
        [InlineData("    ")]
        [InlineData("#This is a comment")]
        [InlineData("   #This is a comment")]
        public void ParsesWithoutError(string input) => ParseInput(input);
            // if there were an error, this test would fail with a DslException

    }
}