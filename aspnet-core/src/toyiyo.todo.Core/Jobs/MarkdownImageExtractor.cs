using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Abp.Domain.Services;

namespace toyiyo.todo.Jobs
{
    public interface IMarkdownImageExtractor
    {
        IEnumerable<MarkdownImage> ExtractImages(string markdown);
        string ReplaceBase64ImagesWithUrls(string markdown, Dictionary<string, string> imageIdMap);
    }

    public class MarkdownImageExtractor : DomainService, IMarkdownImageExtractor
    {
        private static readonly Regex ImagePattern = new(@"!\[([^\]]*)\]\(data:([^;]+);base64,([^\)]+)\)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        public IEnumerable<MarkdownImage> ExtractImages(string markdown)
        {
            if (string.IsNullOrEmpty(markdown)) return Enumerable.Empty<MarkdownImage>();

            var matches = ImagePattern.Matches(markdown);
            return matches.Select(m => new MarkdownImage
            {
                FileName = m.Groups[1].Value,
                ContentType = m.Groups[2].Value,
                Base64Data = m.Groups[3].Value
            });
        }

        public string ReplaceBase64ImagesWithUrls(string markdown, Dictionary<string, JobImage> imageIdMap)
        {
            if (string.IsNullOrEmpty(markdown)) return markdown;

            return ImagePattern.Replace(markdown, m =>
            {
                var altText = m.Groups[1].Value;
                var base64Data = m.Groups[3].Value;
                
                if (imageIdMap.TryGetValue(base64Data, out JobImage existingImage))
                {
                    // Use relative URL format that will work with your API - move this function to a calculated property in the JobImage class
                    var imageTag = $"<img src=\"{existingImage.ImageUrl}\" alt=\"{altText}\" />";
                    return $"[{imageTag}]({existingImage.ImageUrl})";
                }
                
                return m.Value; // Keep original if no mapping found
            });
        }
    }

    public class MarkdownImage
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Base64Data { get; set; }
    }
}