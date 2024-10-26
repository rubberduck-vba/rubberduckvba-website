using Antlr4.Runtime.Misc;
using Rubberduck.Parsing.Grammar;

namespace RubberduckServices.Internal
{
    internal class CommentIntervalsListener : IntervalListener
    {
        public const string DefaultCommentClass = "comment";

        public CommentIntervalsListener(string cssClass = DefaultCommentClass) : base(cssClass) { }

        public override void ExitCommentOrAnnotation(VBAParser.CommentOrAnnotationContext context)
        {
            if (context.annotationList() == null)
            {
                // exclude the line-ending token
                AddInterval(new Interval(context.SourceInterval.a, context.SourceInterval.b));
            }
        }
    }

    internal class NewLineListener : IntervalListener
    {
        public NewLineListener() : base("") { }

        public override void ExitEndOfLine([NotNull] VBAParser.EndOfLineContext context)
        {
            AddInterval(new Interval(context.SourceInterval.a, context.SourceInterval.b));
        }
    }
}
