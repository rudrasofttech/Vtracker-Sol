using System;
using System.Text.RegularExpressions;

namespace VisitTracker.Web.Helpers
{
    /// <summary>
    /// Turns a raw referrer (as stored in Visit.Referer, possibly URL-encoded like
    /// "https%3A%2F%2Fwww.google.com%2F") into a readable group plus the raw host.
    /// </summary>
    public static class ReferrerGrouper
    {
        public const string Direct = "Direct / none";
        public const string Internal = "Internal (same site)";

        private static readonly Regex GoogleSearchHost =
            new(@"^(www\.)?google\.[a-z]{2,3}(\.[a-z]{2})?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Order matters: the first matching rule wins, so specific hosts come before broad ones.
        // A domain matches the host exactly or as a parent domain (l.facebook.com matches facebook.com).
        // Android app referrers (android-app://com.pinterest/) arrive with the app id as the host.
        private static readonly (string Group, string[] Domains)[] Rules =
        [
            ("Google Ads",    ["googleadservices.com", "googlesyndication.com", "doubleclick.net"]),
            ("AI assistants", ["chatgpt.com", "chat.openai.com", "perplexity.ai", "gemini.google.com", "copilot.microsoft.com", "claude.ai"]),
            ("Email",         ["mail.google.com", "com.google.android.gm", "outlook.live.com", "outlook.office.com", "mail.yahoo.com"]),
            ("Google Search", ["com.google.android.googlequicksearchbox", "com.google.android.apps.searchlite"]),
            ("Bing",          ["bing.com"]),
            ("Yahoo",         ["search.yahoo.com", "yahoo.com"]),
            ("DuckDuckGo",    ["duckduckgo.com"]),
            ("Brave Search",  ["search.brave.com"]),
            ("Yandex",        ["yandex.ru", "yandex.com"]),
            ("Ecosia",        ["ecosia.org"]),
            ("Facebook",      ["facebook.com", "fb.com", "com.facebook.katana", "com.facebook.lite"]),
            ("Instagram",     ["instagram.com", "com.instagram.android"]),
            ("X / Twitter",   ["t.co", "twitter.com", "x.com"]),
            ("Pinterest",     ["pinterest.com", "pinterest.co.in", "pin.it", "com.pinterest"]),
            ("YouTube",       ["youtube.com", "youtu.be", "com.google.android.youtube"]),
            ("WhatsApp",      ["whatsapp.com", "wa.me", "com.whatsapp"]),
            ("LinkedIn",      ["linkedin.com", "lnkd.in", "com.linkedin.android"]),
            ("Reddit",        ["reddit.com", "com.reddit.frontpage"]),
            ("Telegram",      ["t.me", "telegram.org", "org.telegram.messenger"]),
            ("Quora",         ["quora.com"]),
            ("WebStats dashboard", ["webstats.co.in"])
        ];

        /// <param name="referer">Raw referrer as stored.</param>
        /// <param name="siteHost">The tracked website's own host (Website.Name), to detect internal navigation.</param>
        public static (string Group, string RawHost) Group(string? referer, string? siteHost = null)
        {
            var host = ExtractHost(referer);
            if (host == null)
            {
                return (Direct, string.Empty);
            }

            var bareHost = StripWww(host);

            if (!string.IsNullOrWhiteSpace(siteHost) &&
                bareHost.Equals(StripWww(siteHost.Trim().ToLowerInvariant()), StringComparison.OrdinalIgnoreCase))
            {
                return (Internal, host);
            }

            foreach (var (group, domains) in Rules)
            {
                foreach (var domain in domains)
                {
                    if (bareHost == domain || bareHost.EndsWith("." + domain, StringComparison.Ordinal))
                    {
                        return (group, host);
                    }
                }
            }

            if (GoogleSearchHost.IsMatch(host))
            {
                return ("Google Search", host);
            }

            // Unknown site: group under its own host so it's still readable.
            return (bareHost, host);
        }

        /// <summary>Decodes the referrer (stored URL-encoded) and returns its lower-case host, or null for direct.</summary>
        private static string? ExtractHost(string? referer)
        {
            if (string.IsNullOrWhiteSpace(referer))
            {
                return null;
            }

            var value = referer.Trim();

            // Values are stored percent-encoded; some may be double-encoded.
            for (var i = 0; i < 2 && value.Contains('%'); i++)
            {
                value = Uri.UnescapeDataString(value);
            }

            if (value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!value.Contains("://", StringComparison.Ordinal))
            {
                value = "https://" + value;
            }

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || string.IsNullOrEmpty(uri.Host))
            {
                return null;
            }

            return uri.Host.ToLowerInvariant();
        }

        private static string StripWww(string host) =>
            host.StartsWith("www.", StringComparison.Ordinal) ? host[4..] : host;
    }
}
