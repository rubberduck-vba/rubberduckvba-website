using HtmlAgilityPack;
using MarkdownSharp;
using RubberduckServices;

namespace rubberduckvba.Server.Services;

public interface IMarkdownFormattingService
{
    string FormatMarkdownDocument(string content, bool withSyntaxHighlighting = false);
}

public class MarkdownFormattingService(ISyntaxHighlighterService service) : IMarkdownFormattingService
{
    private static readonly Markdown _service = new();

    public string FormatMarkdownDocument(string content, bool withSyntaxHighlighting = false)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var markdown = content.Replace("\n", "\r\n");
        var html = string.Empty;

        if (withSyntaxHighlighting)
        {
            // HTML tags makes the markdown formatter silently fail,
            // so we pull out any <code> blocks and format everything between them.

            markdown = PreProcessMarkdownString(markdown);

            var lastSectionStart = markdown.LastIndexOf("</code>");
            if (lastSectionStart > 0)
            {
                var sectionStart = 0;
                while (sectionStart < lastSectionStart)
                {
                    var sectionEnd = markdown.IndexOf("<code", sectionStart) - 1;
                    if (sectionEnd < 0)
                    {
                        sectionEnd = markdown.Length - sectionStart - 1;
                    }

                    var sectionLength = sectionEnd - sectionStart + 1;

                    var section = _service.Transform(markdown.Substring(sectionStart, sectionLength));
                    sectionStart = markdown.IndexOf("</code>", sectionEnd) > 0
                        ? markdown.IndexOf("</code>", sectionEnd) + 8
                        : lastSectionStart;

                    var block = markdown.Substring(sectionEnd, sectionStart - sectionEnd).Replace("<code", "<div").Replace("</code>", "</div>");
                    html += section + block;
                }
            }
            else
            {
                html = _service.Transform(markdown);
            }
        }
        else
        {
            html = _service.Transform(markdown);
        }

        return PostProcessHtml(html);
    }

    private string PreProcessMarkdownString(string content)
    {
        var document = new HtmlDocument();
        document.LoadHtml($"<div>{content}</div>");

        var codeNodes = document.DocumentNode.Descendants("code").ToList();
        foreach (var node in codeNodes)
        {
            var code = service.Format(node.InnerText);

            //node.Name = "div";
            //node.EndNode.Name = "div";
            node.AddClass("vbe-mock-debugger");
            node.InnerHtml = string.Empty;

            var codeDiv = HtmlNode.CreateNode("<div></div>");
            codeDiv.AddClass("code");

            var blockDiv = HtmlNode.CreateNode("<div></div>");
            blockDiv.AddClass("block");
            blockDiv.InnerHtml = code;

            var codeAreaDiv = HtmlNode.CreateNode("<div></div>");
            codeAreaDiv.AddClass("code-area");
            codeAreaDiv.AddClass("text-nowrap");
            codeAreaDiv.AddClass("overflow-auto");

            codeDiv.AppendChild(blockDiv);
            codeAreaDiv.AppendChild(codeDiv);

            node.AppendChild(codeAreaDiv);
        }

        return document.DocumentNode.InnerHtml;
    }

    private string PostProcessHtml(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml($"<div>{html}</div>");

        foreach (var node in document.DocumentNode.Descendants("img").ToList())
        {
            node.AddClass("document-img");
        }

        return document.DocumentNode.FirstChild.InnerHtml;
    }
}
