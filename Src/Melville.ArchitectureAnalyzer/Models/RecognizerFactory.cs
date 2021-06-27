using System.Collections.Generic;
using System.Linq;

namespace ArchitectureAnalyzer.Models
{
    public readonly struct RecognizerFactory
    {
        private readonly Dictionary<string, GlobRecognizer> dict;

        public RecognizerFactory(Dictionary<string, GlobRecognizer> dict)
        {
            this.dict = dict;
        }

        public GlobRecognizer Create(string item) => 
            dict.Memorized(item, i => new GlobRecognizer(item));

        public IEnumerable<GlobRecognizer> MatchingRecognizers(string item) =>
            dict.Values.Where(i => i.Matches(item));
    }
}