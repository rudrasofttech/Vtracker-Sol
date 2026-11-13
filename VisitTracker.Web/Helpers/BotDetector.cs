namespace VisitTracker.Web.Helpers
{
    public static class BotDetector
    {
        private static readonly string[] BotTokens =
        [
            "headlesschrome", "bot", "crawler", "spider", "lighthouse", "phantomjs", "puppeteer", "playwright"
        ];

        private static readonly string[] CommonUaTokens =
        [
            "windows", "macintosh", "linux", "android", "iphone", "ipad", "mobile", "chrome", "safari", "firefox", "edg", "opr", "samsungbrowser"
        ];

        public static bool IsBot(string? userAgent, bool isProxy, int? screenWidth, int? screenHeight)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
            {
                return true;
            }

            var ua = userAgent.ToLowerInvariant();

            if (BotTokens.Any(t => ua.Contains(t, StringComparison.Ordinal)))
            {
                return true;
            }

            if (isProxy)
            {
                return true;
            }

            var suspiciousScreen = (screenWidth == 1600 && screenHeight == 1600) || (screenWidth == 800 && screenHeight == 600);
            if (suspiciousScreen)
            {
                var hasCommonToken = CommonUaTokens.Any(t => ua.Contains(t, StringComparison.Ordinal));
                if (!hasCommonToken)
                {
                    return true;
                }
            }

            return false;
        }
    }
}