using System;
using System.Collections.Generic;
using System.Linq;

namespace ATS5.Application.Authority
{
    public static class AuthorityPermissionSerializer
    {
        public static string SerializeTags(IReadOnlyList<string> tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return "[]";
            }

            return "[" + string.Join(",", tags.Select(tag => $"\"{EscapeJsonString(tag)}\"")) + "]";
        }

        public static IReadOnlyList<string> ParseTags(string? powers)
        {
            if (string.IsNullOrWhiteSpace(powers))
            {
                return Array.Empty<string>();
            }

            var trimmed = powers?.Trim() ?? string.Empty;
            if (trimmed.Length < 2 || trimmed[0] != '[' || trimmed[trimmed.Length - 1] != ']')
            {
                return Array.Empty<string>();
            }

            return trimmed.Substring(1, trimmed.Length - 2)
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim().Trim('"'))
                .Where(item => !string.IsNullOrEmpty(item))
                .ToList();
        }

        private static string EscapeJsonString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
