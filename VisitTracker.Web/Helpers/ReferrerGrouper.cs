namespace VisitTracker.Web.Helpers
{
    public static class ReferrerGrouper
    {
        public static (string Group, string RawHost) Group(string? rawReferrer)
        {
            if (string.IsNullOrWhiteSpace(rawReferrer))
            {
                return ("Direct / none", string.Empty);
            }

            if (!Uri.TryCreate(rawReferrer, UriKind.Absolute, out var uri))
            {
                return ("Direct / none", string.Empty);
            }

            var host = (uri.Host ?? string.Empty).ToLowerInvariant();

            if (host.StartsWith("www.google.") || host == "search.google.com" || host == "com.google.android.googlequicksearchbox")
            {
                return ("Google", host);
            }

            if (uri.Scheme.Equals("android-app", StringComparison.OrdinalIgnoreCase) && rawReferrer.Contains("com.google", StringComparison.OrdinalIgnoreCase))
            {
                return ("Google", host);
            }

            if (host.EndsWith(".bing.com") || host == "bing.com")
            {
                return ("Bing", host);
            }

            if (host == "l.facebook.com" || host == "m.facebook.com" || host == "lm.facebook.com")
            {
                return ("Facebook", host);
            }

            if (host == "l.instagram.com")
            {
                return ("Instagram", host);
            }

            if (host == "t.co")
            {
                return ("Twitter/X", host);
            }

            return (host, host);
        }
    }
}