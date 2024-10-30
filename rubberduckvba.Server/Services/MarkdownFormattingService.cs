using HtmlAgilityPack;
using MarkdownSharp;
using RubberduckServices;

namespace rubberduckvba.com.Server.Services;

public interface IMarkdownFormattingService
{
    Task<string> FormatMarkdownDocument(string content, bool withSyntaxHighlighting = false);
}

public class MarkdownFormattingService(ISyntaxHighlighterService service) : IMarkdownFormattingService
{
    public async Task<string> FormatMarkdownDocument(string content, bool withSyntaxHighlighting = false)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var markdown = content.Replace("\n", "\r\n");
        if (withSyntaxHighlighting)
        {
            markdown = await PreProcessMarkdownString(markdown);
        }

        var html = new Markdown().Transform(markdown);
        var result = await PostProcessHtml(html);

        return await Task.FromResult(result);
    }

    private async Task<string> PreProcessMarkdownString(string content)
    {
        var document = new HtmlDocument();
        document.LoadHtml($"<div>{content}</div>");

        var codeNodes = document.DocumentNode.Descendants("code").ToList();
        foreach (var node in codeNodes)
        {
            var code = await service.FormatAsync(node.InnerText);

            node.Name = "div";
            node.EndNode.Name = "div";
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

    private async Task<string> PostProcessHtml(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml($"<div>{html}</div>");

        foreach (var node in document.DocumentNode.Descendants("img").ToList())
        {
            node.AddClass("document-img");
        }

        return await Task.FromResult(document.DocumentNode.FirstChild.InnerHtml);
    }
}
