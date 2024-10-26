using Antlr4.Runtime.Misc;
using Rubberduck.Parsing.Grammar;

namespace RubberduckServices.Internal
{
    internal class AttributeIntervalsListener : IntervalListener
    {
        public const string DefaultAttributeClass = "attribute";

        public AttributeIntervalsListener(string cssClass = DefaultAttributeClass) : base(cssClass) { }

        public override void ExitAttributeStmt(VBAParser.AttributeStmtContext context)
        {
            // exclude the line-ending token
            AddInterval(new Interval(context.SourceInterval.a, context.SourceInterval.b));
        }
    }
}
