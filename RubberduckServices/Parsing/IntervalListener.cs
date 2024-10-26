using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Rubberduck.Parsing.Grammar;

namespace RubberduckServices.Internal
{
    internal abstract class IntervalListener : VBAParserBaseListener
    {
        private readonly IList<Interval> _intervals = new List<Interval>();

        protected IntervalListener(string cssClass)
        {
            Class = cssClass;
        }

        public string Class { get; }

        public bool IsValidInterval(IToken token, out Interval interval)
        {
            if (!_intervals.Any())
            {
                interval = Interval.Invalid;
                return false;
            }

            interval = ExistsIn(_intervals, token);
            return !interval.Equals(default(Interval));
        }

        private static Interval ExistsIn(IList<Interval> source, IToken token)
        {
            var tokenInterval = new Interval(token.TokenIndex, token.TokenIndex);
            return source.SingleOrDefault(e => e.ProperlyContains(tokenInterval));
        }

        protected void AddInterval(Interval interval)
        {
            if (!_intervals.Any(e => e.ProperlyContains(interval)))
            {
                _intervals.Add(interval);
            }
        }
    }
}
