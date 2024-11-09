using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Rubberduck.Parsing.Grammar;
using Rubberduck.SmartIndenter;
using RubberduckServices.Internal;
using RubberduckServices.Parsing;
using System.Text;

namespace RubberduckServices;

/// <summary>
/// Represents a service that can parse and format a VBA code string.
/// </summary>
public interface ISyntaxHighlighterService
{
    /// <summary>
    /// Formats the specified code.
    /// </summary>
    /// <param name="code">A fragment of VBA code that can be parsed by Rubberduck.</param>
    /// <returns>The provided code, syntax-formatted.</returns>
    string Format(string code);
}

public class SyntaxHighlighterService : ISyntaxHighlighterService
{
    public const string DefaultKeywordClass = "keyword";
    public const string DefaultStringLiteralClass = "string-literal";

    private readonly string _keywordClass;
    private readonly string _stringLiteralClass;
    private readonly string _commentClass;
    private readonly string _annotationClass;
    private readonly string _annotationArgsClass;
    private readonly string _attributeClass;
    private readonly string _attributeValueClass;

    public SyntaxHighlighterService(
        string cssKeywords = DefaultKeywordClass,
        string cssStringLiterals = DefaultStringLiteralClass,
        string cssComments = CommentIntervalsListener.DefaultCommentClass,
        string cssAnnotations = AnnotationIntervalsListener.DefaultAnnotationClass,
        string cssAnnotationArgs = AnnotationArgsIntervalsListener.DefaultAnnotationArgsClass,
        string cssAttributes = AttributeIntervalsListener.DefaultAttributeClass,
        string cssAttributeValues = AttributeValueIntervalsListener.DefaultAttributeValueClass)
    {
        _keywordClass = cssKeywords;
        _stringLiteralClass = cssStringLiterals;
        _commentClass = cssComments;
        _annotationClass = cssAnnotations;
        _annotationArgsClass = cssAnnotationArgs;
        _attributeClass = cssAttributes;
        _attributeValueClass = cssAttributeValues;
    }

    public string Format(string code)
    {
        var indenter = new SimpleIndenter();

        var settings = new IndenterSettingsAdapter(IndenterViewModel.Default);
        var indentedCode = code.Split('\n').SkipWhile(string.IsNullOrWhiteSpace).Select(line => line.Replace("\r", string.Empty)).ToArray();

        try
        {
            indentedCode = indenter.Indent(indentedCode, settings).ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        var builder = new StringBuilder();
        var tokens = Tokenize(string.Join("\n", indentedCode));

        var parser = new VBAParser(tokens)
        {
            Interpreter = { PredictionMode = PredictionMode.Ll } // slow but accurate
        };

        var listeners = new IntervalListener[]
        {
                new CommentIntervalsListener(_commentClass),
                new AnnotationIntervalsListener(_annotationClass),
                new AnnotationArgsIntervalsListener(_annotationArgsClass),
                new AttributeIntervalsListener(_attributeClass),
                new AttributeValueIntervalsListener(_attributeValueClass),
        };

        foreach (var listener in listeners)
        {
            parser.AddParseListener(listener);
        }

        var tree = parser.startRule();
        FormatTokens(builder, tokens, listeners);

        var lines = builder.ToString().Split("\n").ToArray();
        var indent = lines.LastOrDefault()?.TakeWhile(char.IsWhiteSpace)?.Count() ?? 0;
        var formattedLines = from line in lines
                             let trimmed = line[indent..]
                             select FormatIndents(trimmed);

        return string.Join("<br/>", formattedLines);
    }

    private void FormatTokens(StringBuilder builder, ITokenStream tokens, IntervalListener[] listeners)
    {
        for (var i = 0; i < tokens.Size; i++)
        {
            var token = tokens.Get(i);
            var listener = listeners.Select(e => new
            {
                IsValidInterval = e.IsValidInterval(token, out var interval),
                Interval = interval,
                e.Class
            }).FirstOrDefault(e => e.IsValidInterval);

            if (listener != null && !string.IsNullOrWhiteSpace(listener.Class))
            {
                builder.Append($"<span class=\"{listener.Class}\">{tokens.GetText(listener.Interval)}</span>");
                i = listener.Interval.b;
            }
            //else if (listener is NewLineListener)
            //{
            //    if (token.Type == VBAParser.NEWLINE)
            //    {
            //        builder.Append(Environment.NewLine);
            //    }
            //}
            else
            {
                if (TokenKinds.StringLiterals.Contains(token.Type))
                {
                    builder.Append($"<span class=\"{_stringLiteralClass}\">{token.Text}</span>");
                }
                else if (TokenKinds.Keywords.Contains(token.Type))
                {
                    builder.Append($"<span class=\"{_keywordClass}\">{token.Text}</span>");
                }
                else if (token.Type != VBAParser.Eof)
                {
                    builder.Append(token.Text);
                }
                else
                {
                    builder.Append($"<br/><span>&nbsp;</span>");
                }
            }
        }
    }

    private static CommonTokenStream Tokenize(string code)
    {
        AntlrInputStream input;
        using (var reader = new StringReader(code))
        {
            input = new AntlrInputStream(reader);
        }
        var lexer = new VBALexer(input);
        return new CommonTokenStream(lexer);
    }

    private static string FormatIndents(string line)
    {
        var formatted = line;
        var indent = line.TakeWhile(char.IsWhiteSpace).Count();
        if (indent > 0)
        {
            formatted = string.Concat(line[..indent].Replace(" ", "&nbsp;"), line.AsSpan(indent));
        }
        return formatted;
    }
}
