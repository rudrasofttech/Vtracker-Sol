using System.Globalization;
using VisitTracker.Models;

namespace VisitTracker.Web.Helpers
{
    /// <summary>
    /// Classification rules shared by the Report, Visits and Page behaviour pages,
    /// so every page labels channels, devices, browsers and clicks the same way.
    /// </summary>
    public static class VisitAnalytics
    {
        private static readonly HashSet<string> SearchGroups = new(StringComparer.OrdinalIgnoreCase)
        { "Google Search", "Google", "Bing", "Yahoo", "DuckDuckGo", "Brave Search", "Yandex", "Ecosia" };

        private static readonly HashSet<string> SocialGroups = new(StringComparer.OrdinalIgnoreCase)
        { "Facebook", "Instagram", "X / Twitter", "Twitter/X", "Pinterest", "YouTube", "WhatsApp", "LinkedIn", "Reddit", "Telegram", "Quora" };

        private static readonly HashSet<string> SocialUtmSources = new(StringComparer.OrdinalIgnoreCase)
        { "facebook", "fb", "instagram", "ig", "meta", "whatsapp", "pinterest", "youtube", "twitter", "x", "linkedin", "telegram" };

        private static readonly HashSet<string> TrackingParams = new(StringComparer.OrdinalIgnoreCase)
        { "gclid", "gad_source", "gad_campaignid", "gbraid", "wbraid", "fbclid", "msclkid", "srsltid", "_gl", "igshid", "mc_cid", "mc_eid", "ref_src" };

        /// <summary>Elements that respond to taps on their own. Taps on anything else without a link are "dead clicks".</summary>
        private static readonly HashSet<string> InteractiveTags = new(StringComparer.OrdinalIgnoreCase)
        { "A", "BUTTON", "INPUT", "SELECT", "TEXTAREA", "LABEL", "OPTION", "SUMMARY", "VIDEO", "AUDIO", "IFRAME" };

        // ---------- Channels ----------

        public static string Channel(string referrerGroup, string? utmSource, string? utmMedium, string? userAgent, string? landingQuery)
        {
            var q = (landingQuery ?? string.Empty).ToLowerInvariant();
            var medium = (utmMedium ?? string.Empty).Trim().ToLowerInvariant();
            var source = (utmSource ?? string.Empty).Trim();

            var paidClick = q.Contains("gclid=") || q.Contains("gad_source=") || q.Contains("gbraid=")
                || q.Contains("wbraid=") || q.Contains("msclkid=") || referrerGroup == "Google Ads";
            var paidMedium = medium is "cpc" or "ppc" or "cpm" or "paid" || medium.StartsWith("paid");
            var social = SocialGroups.Contains(referrerGroup) || SocialUtmSources.Contains(source)
                || q.Contains("fbclid=") || IsSocialInApp(userAgent);

            if (paidClick) return "Paid search";
            if (paidMedium) return social ? "Paid social" : "Paid search";
            if (medium == "email" || referrerGroup == "Email") return "Email";
            if (SearchGroups.Contains(referrerGroup)) return "Organic search";
            if (social) return "Social";
            if (referrerGroup == "AI assistants") return "AI assistants";
            if (!string.IsNullOrEmpty(source)) return "Other campaigns";
            if (referrerGroup == ReferrerGrouper.Direct || referrerGroup == ReferrerGrouper.Internal || referrerGroup == "WebStats dashboard")
                return "Direct";
            return "Referral";
        }

        // ---------- Devices ----------

        public static string DeviceType(int? width, int? height, string? ua)
        {
            ua ??= string.Empty;
            if (ua.Contains("iPad") || (ua.Contains("Android") && !ua.Contains("Mobile"))) return "Tablet";
            if (ua.Contains("iPhone") || ua.Contains("Mobi")) return "Mobile";
            if (!width.HasValue || width <= 0) return "Unknown";
            if (width < 600) return "Mobile";
            if (width < 1100 && height > width) return "Tablet";
            return "Desktop";
        }

        public static string OperatingSystem(string? ua)
        {
            if (string.IsNullOrWhiteSpace(ua)) return "Not recorded";
            if (ua.Contains("Android")) return "Android";
            if (ua.Contains("iPhone") || ua.Contains("iPad") || ua.Contains("iPod")) return "iOS";
            if (ua.Contains("Windows")) return "Windows";
            if (ua.Contains("CrOS")) return "ChromeOS";
            if (ua.Contains("Mac OS X")) return "macOS";
            if (ua.Contains("Linux")) return "Linux";
            return "Other";
        }

        public static string Browser(string? ua, string? browserName)
        {
            if (string.IsNullOrWhiteSpace(ua))
            {
                return string.IsNullOrWhiteSpace(browserName) ? "Other" : browserName;
            }

            if (ua.Contains("Instagram")) return "Instagram (in-app)";
            if (ua.Contains("FBAN") || ua.Contains("FBAV") || ua.Contains("FB_IAB")) return "Facebook (in-app)";
            if (ua.Contains("Pinterest")) return "Pinterest (in-app)";
            if (ua.Contains("SamsungBrowser")) return "Samsung Internet";
            if (ua.Contains("Edg/")) return "Edge";
            if (ua.Contains("OPR/") || ua.Contains("Opera")) return "Opera";
            if (ua.Contains("; wv)")) return "Android WebView";
            if (ua.Contains("CriOS")) return "Chrome";
            if (ua.Contains("FxiOS") || ua.Contains("Firefox")) return "Firefox";
            if (ua.Contains("Chrome")) return "Chrome";
            if ((ua.Contains("iPhone") || ua.Contains("iPad")) && !ua.Contains("Safari")) return "iOS in-app";
            if (ua.Contains("Safari")) return "Safari";
            return "Other";
        }

        public static bool IsSocialInApp(string? ua) =>
            !string.IsNullOrEmpty(ua) &&
            (ua.Contains("Instagram") || ua.Contains("FBAN") || ua.Contains("FBAV") || ua.Contains("FB_IAB") || ua.Contains("Pinterest"));

        public static string Location(string? city, string? region)
        {
            var c = Clean(city);
            var r = Clean(region);
            if (c == null && r == null) return "Unknown";
            if (c == null) return r!;
            if (r == null || r.Equals(c, StringComparison.OrdinalIgnoreCase)) return c;
            return $"{c}, {r}";
        }

        public static string Blank(string? value) => Clean(value) ?? "Unknown";

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) || value.Trim() == "-" ? null : value.Trim();

        // ---------- Pages ----------

        /// <summary>Removes ad and analytics parameters (utm_*, gclid, gad_source, fbclid…) from a query string.</summary>
        public static string StripTracking(string? query)
        {
            if (string.IsNullOrWhiteSpace(query)) return string.Empty;

            var kept = query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Where(p =>
                {
                    var name = p.Split('=')[0];
                    return !name.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) && !TrackingParams.Contains(name);
                });

            var result = string.Join("&", kept);
            return result.Length == 0 ? string.Empty : "?" + result;
        }

        public static string PageKey(string? path, string? query) =>
            (string.IsNullOrEmpty(path) ? "/" : path) + StripTracking(query);

        // ---------- Clicks ----------

        public static (string Kind, string Target) ClassifyClick(string href, string siteHost)
        {
            var h = href.Trim();

            if (h.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)) return ("Call", h[4..]);
            if (h.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)) return ("Email", h[7..].Split('?')[0]);
            if (h.Contains("wa.me", StringComparison.OrdinalIgnoreCase) || h.Contains("whatsapp", StringComparison.OrdinalIgnoreCase))
                return ("WhatsApp", h.Split('?')[0]);
            if (h.StartsWith('#') || h.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)) return ("In-page", h);

            if (Uri.TryCreate(h, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                var host = StripWww(uri.Host);
                if (host.Equals(StripWww(siteHost), StringComparison.OrdinalIgnoreCase))
                {
                    return ("Link", uri.AbsolutePath + StripTracking(uri.Query));
                }
                return ("Outbound", host + uri.AbsolutePath.TrimEnd('/'));
            }

            var qIndex = h.IndexOf('?');
            var path = qIndex >= 0 ? h[..qIndex] : h;
            var query = qIndex >= 0 ? h[qIndex..] : string.Empty;
            return ("Link", (path.StartsWith('/') ? path : "/" + path) + StripTracking(query));
        }

        public static bool IsDeadClick(string? tag, string? href) =>
            string.IsNullOrWhiteSpace(href) && !InteractiveTags.Contains((tag ?? string.Empty).Trim());

        /// <summary>A readable name for a tapped element, e.g. img#main-photo or a “Buy now”.</summary>
        public static string ElementLabel(string? tag, string? tagId, string? text)
        {
            var label = string.IsNullOrWhiteSpace(tag) ? "element" : tag.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(tagId)) label += tagId.Trim().StartsWith('#') ? tagId.Trim() : "#" + tagId.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                var t = text.Trim();
                if (t.Length > 40) t = t[..40] + "…";
                label += $" “{t}”";
            }
            return label;
        }

        /// <summary>
        /// Groups taps on one page view into rage-click bursts: 3 or more taps, each within
        /// 2 seconds of the previous one and within 40 px of the first tap of the burst.
        /// </summary>
        public static List<List<ActivityRow>> FindRageClusters(IEnumerable<ActivityRow> clicks)
        {
            var result = new List<List<ActivityRow>>();

            foreach (var group in clicks.Where(c => c.Activity == ActivityName.Click).GroupBy(c => c.VisitPageID))
            {
                List<ActivityRow>? current = null;

                foreach (var c in group.OrderBy(c => c.DateCreated).ThenBy(c => c.ID))
                {
                    var joins = current != null
                        && (c.DateCreated - current[^1].DateCreated).TotalSeconds <= 2
                        && Math.Abs((c.X ?? 0) - (current[0].X ?? 0)) <= 40
                        && Math.Abs((c.Y ?? 0) - (current[0].Y ?? 0)) <= 40;

                    if (joins)
                    {
                        current!.Add(c);
                    }
                    else
                    {
                        if (current is { Count: >= 3 }) result.Add(current);
                        current = [c];
                    }
                }

                if (current is { Count: >= 3 }) result.Add(current);
            }

            return result;
        }

        // ---------- Formatting ----------

        public static string Duration(double seconds)
        {
            if (seconds < 1) return "0s";
            if (seconds < 60) return $"{seconds:0}s";
            var m = (int)(seconds / 60);
            var s = (int)(seconds % 60);
            if (m < 60) return $"{m}m {s:00}s";
            return $"{m / 60}h {m % 60:00}m";
        }

        public static double Pct(int part, int total) => total == 0 ? 0 : part * 100.0 / total;

        public static string Inv(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        public static string StripWww(string host) =>
            host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? host[4..] : host;

        // ---------- Public visit reference ----------
        // Visit pages are public for now, so visits are referred to by a scrambled code
        // instead of the database ID. The mapping is reversible: id * K mod 2^32, in base 36.

        private const uint RefMultiplier = 2654435761u;
        private const uint RefInverse = 244002641u;
        private const string Base36 = "0123456789abcdefghijklmnopqrstuvwxyz";

        public static string VisitRef(int id)
        {
            var x = unchecked((uint)id * RefMultiplier);
            Span<char> buffer = stackalloc char[8];
            var i = buffer.Length;
            do
            {
                buffer[--i] = Base36[(int)(x % 36)];
                x /= 36;
            } while (x > 0);
            return new string(buffer[i..]);
        }

        public static int? VisitIdFromRef(string? reference)
        {
            if (string.IsNullOrWhiteSpace(reference) || reference.Length > 8) return null;

            ulong x = 0;
            foreach (var ch in reference.Trim().ToLowerInvariant())
            {
                var d = Base36.IndexOf(ch);
                if (d < 0) return null;
                x = x * 36 + (ulong)d;
            }
            if (x > uint.MaxValue) return null;

            var id = unchecked((uint)x * RefInverse);
            return id is > 0 and <= int.MaxValue ? (int)id : null;
        }
    }
}
