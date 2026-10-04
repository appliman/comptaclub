using System.Text.RegularExpressions;
using Markdig;

namespace ComptaClub.Blazor.Services.Documentation;

public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline PIPELINE = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseAutoLinks()
        .UseBootstrap()
        .Build();

    private static readonly Regex LINK_REGEX = new(
        @"<a\s+(?<pre>[^>]*?)href=""(?<href>[^""]+)""(?<post>[^>]*?)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ALERT_REGEX = new(
        @"<div class=""markdown-alert markdown-alert-(?<type>note|tip|important|warning|caution) alert [^""]*"" role=""alert"">\s*(?<rest>.*?)\s*</div>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex IMAGE_REGEX = new(
        @"<img\s+(?<pre>[^>]*?)src=""(?<src>[^""]+)""(?<post>[^>]*?)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var _html = Markdown.ToHtml(markdown, PIPELINE);
        _html = RewriteLinks(_html);
        _html = TransformAlerts(_html);
        _html = EnhanceTables(_html);
        _html = EnhanceImages(_html);

        return _html;
    }

    private static string RewriteLinks(string html)
    {
        return LINK_REGEX.Replace(html, match =>
        {
            var _pre = match.Groups["pre"].Value;
            var _href = match.Groups["href"].Value;
            var _post = match.Groups["post"].Value;

            if (_href.StartsWith('#'))
            {
                return match.Value;
            }

            if (_href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                _href.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                if (_href.Contains("github.com/appliman/comptaclub/wiki/", StringComparison.OrdinalIgnoreCase))
                {
                    var _pageName = _href.Split("github.com/appliman/comptaclub/wiki/", StringSplitOptions.None)[^1]
                        .TrimEnd('/');
                    return $"<a {_pre}href=\"/documentation/{_pageName}\"{_post}>";
                }

                if (!_post.Contains("target=", StringComparison.OrdinalIgnoreCase) &&
                    !_pre.Contains("target=", StringComparison.OrdinalIgnoreCase))
                {
                    return $"<a {_pre}href=\"{_href}\" target=\"_blank\" rel=\"noopener noreferrer\"{_post}>";
                }

                return match.Value;
            }

            if (_href.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                return match.Value;
            }

            var _slug = _href;
            if (_slug.StartsWith("wiki/", StringComparison.OrdinalIgnoreCase))
            {
                _slug = _slug[5..];
            }
            else if (_slug.StartsWith("/wiki/", StringComparison.OrdinalIgnoreCase))
            {
                _slug = _slug[6..];
            }

            _slug = _slug.TrimStart('/');
            if (string.IsNullOrWhiteSpace(_slug))
            {
                _slug = "Home";
            }

            return $"<a {_pre}href=\"/documentation/{_slug}\"{_post}>";
        });
    }

    private static string TransformAlerts(string html)
    {
        return ALERT_REGEX.Replace(html, match =>
        {
            var _alertType = match.Groups["type"].Value.ToUpperInvariant();
            var _content = match.Groups["rest"].Value.Trim();

            if (_content.StartsWith("<br />", StringComparison.OrdinalIgnoreCase))
            {
                _content = _content[6..].Trim();
            }
            else if (_content.StartsWith("<br>", StringComparison.OrdinalIgnoreCase))
            {
                _content = _content[4..].Trim();
            }

            if (!_content.EndsWith("</p>", StringComparison.OrdinalIgnoreCase))
            {
                _content += "</p>";
            }

            var (_cssClass, _icon, _title) = _alertType switch
            {
                "NOTE" => ("alert-info", "fa-circle-info", "Remarque"),
                "TIP" => ("alert-success", "fa-lightbulb", "Conseil"),
                "IMPORTANT" => ("alert-primary", "fa-circle-exclamation", "Important"),
                "WARNING" => ("alert-warning", "fa-triangle-exclamation", "Avertissement"),
                "CAUTION" => ("alert-danger", "fa-circle-xmark", "Attention"),
                _ => ("alert-secondary", "fa-info", "Information")
            };

            return $"""
                <div class="alert {_cssClass} d-flex align-items-start my-3" role="alert">
                    <i class="fa-solid {_icon} fs-5 me-3 mt-1"></i>
                    <div>
                        <div class="fw-bold mb-1">{_title}</div>
                        {_content}
                    </div>
                </div>
                """;
        });
    }

    private static string EnhanceTables(string html)
    {
        if (!html.Contains("<table"))
        {
            return html;
        }

        return html
            .Replace("<table class=\"table\">", "<div class=\"table-responsive my-3\"><table class=\"table table-striped table-hover table-bordered\">")
            .Replace("<table class=\"table", "<div class=\"table-responsive my-3\"><table class=\"table table-striped table-hover table-bordered")
            .Replace("</table>", "</table></div>");
    }

    private static string EnhanceImages(string html)
    {
        return IMAGE_REGEX.Replace(html, match =>
        {
            var _pre = match.Groups["pre"].Value;
            var _src = match.Groups["src"].Value;
            var _post = match.Groups["post"].Value;

            if (_src.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            {
                _src = "/" + _src;
            }
            else if (_src.Contains("github.com/appliman/comptaclub/wiki/images/", StringComparison.OrdinalIgnoreCase) ||
                     _src.Contains("raw.githubusercontent.com/wiki/appliman/comptaclub/images/", StringComparison.OrdinalIgnoreCase))
            {
                var _fileName = _src.Split("/images/", StringSplitOptions.None)[^1];
                _src = $"/images/{_fileName}";
            }

            return $"<img {_pre}src=\"{_src}\" class=\"img-fluid rounded border shadow-sm my-3\"{_post}>";
        });
    }
}
