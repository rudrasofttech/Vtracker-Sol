using System.Text;

namespace VisitTracker.Web.Helpers
{
    public sealed class NormalizedUrlResult
    {
        public string Path { get; init; } = "/";
        public string QueryString { get; init; } = string.Empty;
        public string? UtmSource { get; init; }
        public string? UtmMedium { get; init; }
        public string? UtmCampaign { get; init; }
    }

    public static class UrlNormalizer
    {
        private static readonly HashSet<string> StripKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "fbclid", "gclid", "msclkid", "yclid", "mc_cid", "mc_eid", "_ga", "ref", "igshid"
        };

        public static bool TryNormalize(string? rawUrl, out NormalizedUrlResult result)
        {
            result = new NormalizedUrlResult();

            if (string.IsNullOrWhiteSpace(rawUrl) || !Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var path = (uri.AbsolutePath ?? "/").ToLowerInvariant();
            if (path.Length > 1 && path.EndsWith('/'))
            {
                path = path.TrimEnd('/');
            }

            var kept = new List<KeyValuePair<string, string>>();
            string? utmSource = null;
            string? utmMedium = null;
            string? utmCampaign = null;

            var query = uri.Query.TrimStart('?');
            if (!string.IsNullOrWhiteSpace(query))
            {
                var parts = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var eqIndex = part.IndexOf('=');
                    var rawKey = eqIndex >= 0 ? part[..eqIndex] : part;
                    var rawVal = eqIndex >= 0 ? part[(eqIndex + 1)..] : string.Empty;

                    var key = Uri.UnescapeDataString(rawKey.Replace("+", " "));
                    var val = Uri.UnescapeDataString(rawVal.Replace("+", " "));
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    var lowerKey = key.ToLowerInvariant();

                    if (lowerKey == "utm_source")
                    {
                        utmSource ??= val;
                    }
                    else if (lowerKey == "utm_medium")
                    {
                        utmMedium ??= val;
                    }
                    else if (lowerKey == "utm_campaign")
                    {
                        utmCampaign ??= val;
                    }

                    if (lowerKey.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) || StripKeys.Contains(lowerKey))
                    {
                        continue;
                    }

                    kept.Add(new KeyValuePair<string, string>(lowerKey, val));
                }
            }

            var normalizedQuery = string.Empty;
            if (kept.Count > 0)
            {
                kept = kept
                    .OrderBy(k => k.Key, StringComparer.Ordinal)
                    .ThenBy(v => v.Value, StringComparer.Ordinal)
                    .ToList();

                var sb = new StringBuilder("?");
                for (var i = 0; i < kept.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append('&');
                    }

                    sb.Append(Uri.EscapeDataString(kept[i].Key));
                    sb.Append('=');
                    sb.Append(Uri.EscapeDataString(kept[i].Value));
                }

                normalizedQuery = sb.ToString();
            }

            result = new NormalizedUrlResult
            {
                Path = path,
                QueryString = normalizedQuery,
                UtmSource = string.IsNullOrWhiteSpace(utmSource) ? null : utmSource,
                UtmMedium = string.IsNullOrWhiteSpace(utmMedium) ? null : utmMedium,
                UtmCampaign = string.IsNullOrWhiteSpace(utmCampaign) ? null : utmCampaign
            };

            return true;
        }
    }
}