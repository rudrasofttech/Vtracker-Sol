using VisitTracker.Models;

namespace VisitTracker.Web.Helpers
{
    public sealed record VisitSummary(
        string Ref,
        DateTime LocalStart,
        double Seconds,
        int Pages,
        string Channel,
        string Source,
        string Landing,
        string Device,
        string OperatingSystem,
        string Browser,
        string Location,
        bool Returning,
        IReadOnlyList<string> Badges);

    /// <summary>Kind drives the timeline icon colour: link, contact, tap, dead, rage, away, back, bottom.</summary>
    public sealed record JourneyEvent(double OffsetSeconds, string Kind, string Title, string? Detail, bool Warning);

    public sealed record JourneyPage(
        string Page,
        string LiveUrl,
        string BehaviourUrl,
        double OffsetSeconds,
        double Seconds,
        int? Scroll,
        bool IsExit,
        IReadOnlyList<JourneyEvent> Events);

    public sealed record VisitJourney(
        VisitSummary Summary,
        string ReferrerText,
        string? Campaign,
        string VisitNumberText,
        string ScreenText,
        IReadOnlyList<JourneyPage> Pages);

    public static class VisitJourneys
    {
        public static VisitSummary Summarize(
            VisitRow v,
            IReadOnlyList<PageViewRow> pages,
            IReadOnlyList<ActivityRow> activities,
            string siteHost,
            Func<DateTime, DateTime> toLocal)
        {
            var (group, _) = ReferrerGrouper.Group(v.Referer, siteHost);
            var landing = pages.Count > 0 ? pages[0] : null;

            return new VisitSummary(
                Ref: VisitAnalytics.VisitRef(v.ID),
                LocalStart: toLocal(v.DateCreated),
                Seconds: v.Seconds,
                Pages: pages.Count,
                Channel: VisitAnalytics.Channel(group, v.UtmSource, v.UtmMedium, v.UserAgent, landing?.Query),
                Source: group == ReferrerGrouper.Direct ? "Direct" : group,
                Landing: landing?.Key ?? "–",
                Device: VisitAnalytics.DeviceType(v.ScreenWidth, v.ScreenHeight, v.UserAgent),
                OperatingSystem: VisitAnalytics.OperatingSystem(v.UserAgent),
                Browser: VisitAnalytics.Browser(v.UserAgent, v.BrowserName),
                Location: VisitAnalytics.Location(v.CityName, v.RegionName),
                Returning: v.LastVisitID.HasValue,
                Badges: Badges(v, pages, activities, siteHost));
        }

        private static List<string> Badges(VisitRow v, IReadOnlyList<PageViewRow> pages, IReadOnlyList<ActivityRow> activities, string siteHost)
        {
            var badges = new List<string>();

            var kinds = activities
                .Where(a => a.Activity == ActivityName.Click && !string.IsNullOrWhiteSpace(a.Href))
                .Select(a => VisitAnalytics.ClassifyClick(a.Href!, siteHost).Kind)
                .ToHashSet();

            if (kinds.Contains("WhatsApp")) badges.Add("WhatsApp tap");
            if (kinds.Contains("Call")) badges.Add("Call tap");
            if (kinds.Contains("Email")) badges.Add("Email tap");
            if (kinds.Contains("Outbound")) badges.Add("Outbound click");
            if (activities.Any(a => a.Activity == ActivityName.ScrollBottom)) badges.Add("Reached bottom");
            if (VisitAnalytics.FindRageClusters(activities).Count > 0) badges.Add("Rage taps");
            if (pages.Count <= 1 && v.Seconds < 10) badges.Add("Left quickly");

            return badges;
        }

        public static VisitJourney Build(
            VisitRow v,
            IReadOnlyList<PageViewRow> pages,
            IReadOnlyList<ActivityRow> activities,
            string siteHost,
            Func<DateTime, DateTime> toLocal,
            int? visitNumber,
            Func<string, string> behaviourUrl)
        {
            var summary = Summarize(v, pages, activities, siteHost, toLocal);

            // Activities saved without a page are attached to the page that was open at the time.
            int? PageFor(ActivityRow a) =>
                a.VisitPageID ?? pages.LastOrDefault(p => p.DateCreated <= a.DateCreated)?.ID ?? pages.FirstOrDefault()?.ID;

            var byPage = activities
                .GroupBy(PageFor)
                .ToDictionary(g => g.Key ?? 0, g => g.OrderBy(a => a.DateCreated).ThenBy(a => a.ID).ToList());

            var rage = VisitAnalytics.FindRageClusters(activities);
            var rageByClick = rage.SelectMany(c => c.Select(a => (a.ID, Cluster: c))).ToDictionary(x => x.ID, x => x.Cluster);

            var journeyPages = pages.Select((p, i) =>
            {
                var events = new List<JourneyEvent>();
                foreach (var a in byPage.GetValueOrDefault(p.ID) ?? [])
                {
                    var offset = Math.Max(0, (a.DateCreated - v.DateCreated).TotalSeconds);

                    if (rageByClick.TryGetValue(a.ID, out var cluster))
                    {
                        if (cluster[0].ID == a.ID)
                        {
                            events.Add(new JourneyEvent(offset, "rage",
                                $"Rage-tapped {VisitAnalytics.ElementLabel(a.Tag, a.TagId, a.Text)} ×{cluster.Count}",
                                "Several quick taps in the same spot usually mean something didn't respond.", true));
                        }
                        continue;
                    }

                    var ev = Describe(a, offset, siteHost);
                    if (ev != null) events.Add(ev);
                }

                return new JourneyPage(
                    Page: p.Key,
                    LiveUrl: "https://" + siteHost + p.Key,
                    BehaviourUrl: behaviourUrl("https://" + siteHost + p.Key),
                    OffsetSeconds: Math.Max(0, (p.DateCreated - v.DateCreated).TotalSeconds),
                    Seconds: p.Seconds,
                    Scroll: p.MaxScrollPct,
                    IsExit: i == pages.Count - 1,
                    Events: events);
            }).ToList();

            var (group, host) = ReferrerGrouper.Group(v.Referer, siteHost);
            var referrerText = group == ReferrerGrouper.Direct
                ? "No referrer (typed address, bookmark or an app)"
                : string.IsNullOrEmpty(host) || host.Equals(group, StringComparison.OrdinalIgnoreCase) ? group : $"{group} ({host})";

            string? campaign = string.IsNullOrWhiteSpace(v.UtmSource) && string.IsNullOrWhiteSpace(v.UtmCampaign)
                ? null
                : $"{v.UtmCampaign ?? "(no campaign name)"} ({v.UtmSource ?? "?"} / {v.UtmMedium ?? "?"})";

            var visitNumberText = visitNumber.HasValue
                ? (visitNumber == 1 ? "First visit" : $"{Ordinal(visitNumber.Value)} visit")
                : v.LastVisitID.HasValue ? "Returning visitor" : "First visit";

            var screen = v.ScreenWidth > 0 && v.ScreenHeight > 0 ? $"{v.ScreenWidth} × {v.ScreenHeight}" : "Unknown";

            return new VisitJourney(summary, referrerText, campaign, visitNumberText, screen, journeyPages);
        }

        private static JourneyEvent? Describe(ActivityRow a, double offset, string siteHost)
        {
            switch (a.Activity)
            {
                case ActivityName.Click:
                    {
                        var text = string.IsNullOrWhiteSpace(a.Text) ? null : $"“{a.Text!.Trim()}”";

                        if (!string.IsNullOrWhiteSpace(a.Href))
                        {
                            var (kind, target) = VisitAnalytics.ClassifyClick(a.Href!, siteHost);
                            return kind switch
                            {
                                "WhatsApp" => new JourneyEvent(offset, "contact", "Tapped WhatsApp", Join(text, target), false),
                                "Call" => new JourneyEvent(offset, "contact", "Tapped to call", Join(text, target), false),
                                "Email" => new JourneyEvent(offset, "contact", "Tapped to email", Join(text, target), false),
                                "Outbound" => new JourneyEvent(offset, "link", "Clicked a link to another site", Join(text, target), false),
                                "In-page" => new JourneyEvent(offset, "tap", "Tapped an in-page link", Join(text, target), false),
                                _ => new JourneyEvent(offset, "link", "Clicked a link", Join(text, target), false)
                            };
                        }

                        var label = VisitAnalytics.ElementLabel(a.Tag, a.TagId, a.Text);
                        return VisitAnalytics.IsDeadClick(a.Tag, a.Href)
                            ? new JourneyEvent(offset, "dead", $"Tapped {label}", "Not a link or form control", true)
                            : new JourneyEvent(offset, "tap", $"Tapped {label}", null, false);
                    }
                case ActivityName.WindowBlur:
                    return new JourneyEvent(offset, "away", "Switched to another tab or app", null, false);
                case ActivityName.WindowFocus:
                    return new JourneyEvent(offset, "back", "Came back",
                        a.Seconds is > 0 ? $"after {VisitAnalytics.Duration(a.Seconds.Value)}" : null, false);
                case ActivityName.ScrollBottom:
                    return new JourneyEvent(offset, "bottom", "Reached the bottom of the page", null, false);
                default:
                    return null;
            }
        }

        private static string? Join(string? a, string? b) =>
            string.IsNullOrWhiteSpace(a) ? b : string.IsNullOrWhiteSpace(b) ? a : $"{a} → {b}";

        private static string Ordinal(int n)
        {
            var suffix = (n % 100) is 11 or 12 or 13 ? "th" : (n % 10) switch { 1 => "st", 2 => "nd", 3 => "rd", _ => "th" };
            return n + suffix;
        }
    }
}
