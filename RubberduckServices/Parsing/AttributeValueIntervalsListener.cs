using System.Linq;
using Antlr4.Runtime.Misc;
using Rubberduck.Parsing.Grammar;

namespace RubberduckServices.Internal
{
    internal class AttributeValueIntervalsListener : IntervalListener
    {
        public const string DefaultAttributeValueClass = "attribute-value";

        public AttributeValueIntervalsListener(string cssClass = DefaultAttributeValueClass) : base(cssClass) { }

        public override void ExitAttributeStmt(VBAParser.AttributeStmtContext context)
        {
            var value = context.attributeValue();
            if (value != null)
            {
                // exclude the line-ending token
                AddInterval(new Interval(value.First().SourceInterval.a, value.Last().SourceInterval.b));
            }
        }
    }
}
