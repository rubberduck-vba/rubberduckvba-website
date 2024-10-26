using Antlr4.Runtime.Misc;
using Rubberduck.Parsing.Grammar;

namespace RubberduckServices.Internal
{
    internal class AnnotationIntervalsListener : IntervalListener
    {
        public const string DefaultAnnotationClass = "annotation";

        public AnnotationIntervalsListener(string cssClass = DefaultAnnotationClass) : base(cssClass) { }

        public override void ExitAnnotationName([NotNull] VBAParser.AnnotationNameContext context)
        {
            AddInterval(new Interval(context.SourceInterval.a, context.SourceInterval.b));
        }
    }

    internal class AnnotationArgsIntervalsListener : IntervalListener
    {
        public const string DefaultAnnotationArgsClass = "annotation-args";

        public AnnotationArgsIntervalsListener(string cssClass = DefaultAnnotationArgsClass) : base(cssClass) { }

        public override void ExitAnnotationArgList([NotNull] VBAParser.AnnotationArgListContext context)
        {
            AddInterval(new Interval(context.SourceInterval.a, context.SourceInterval.b));
        }
    }
}
