using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using VisitTracker.DataContext;
using VisitTracker.Models;

namespace VisitTracker.Web.Pages
{
    public sealed record RangePickerModel(
        int Id,
        string SelectedRange,
        DateTime Start,
        DateTime EndInclusive,
        bool IncludeBots,
        IReadOnlyDictionary<string, string> Hidden);

    /// <summary>
    /// Shared plumbing for Report, Visits and PageReport: website lookup, time zone,
    /// date range presets and links that keep the selected range.
    /// </summary>
    public abstract class ReportPageBase(VisitTrackerDBContext db) : PageModel
    {
        protected readonly VisitTrackerDBContext Db = db;
        private readonly WebsiteManager websites = new(db);

        public const string DefaultRange = "last14";
        public const string CustomRange = "custom";

        public static readonly IReadOnlyList<(string Key, string Label)> RangeOptions =
        [
            ("today", "Today"),
            ("yesterday", "Yesterday"),
            ("last24h", "Last 24 hours"),
            ("last7", "Last 7 days"),
            ("thisweek", "This week"),
            ("lastweek", "Last week"),
            ("last14", "Last 14 days"),
            ("last30", "Last 30 days"),
            ("thismonth", "This month"),
            ("lastmonth", "Last month"),
            ("last90", "Last 90 days"),
            (CustomRange, "Custom range")
        ];

        [FromQuery(Name = "id")] public int Id { get; set; }
        [FromQuery(Name = "range")] public string? Range { get; set; }
        [FromQuery(Name = "start")] public DateTime? Start { get; set; }
        [FromQuery(Name = "end")] public DateTime? End { get; set; }
        [FromQuery(Name = "bots")] public int? Bots { get; set; }

        public bool IncludeBots => Bots == 1;

        public string WebsiteName { get; private set; } = string.Empty;
        public string SiteHost { get; private set; } = string.Empty;
        public string SelectedRange { get; private set; } = DefaultRange;
        public string RangeDescription { get; private set; } = string.Empty;
        public DateTime RangeStart { get; private set; }
        public DateTime RangeEndInclusive { get; private set; }

        protected TimeZoneInfo Tz { get; private set; } = TimeZoneInfo.Utc;
        protected DateTime NowLocal { get; private set; }
        protected DateTime LocalStart { get; private set; }
        protected DateTime LocalEnd { get; private set; }
        protected ReportDateRangeCustomType Bucket { get; private set; }
        protected DateTime UtcStart => ToUtc(LocalStart);
        protected DateTime UtcEnd => ToUtc(LocalEnd);

        /// <summary>Loads the website from ?id= and sets its time zone. Returns null if it doesn't exist.</summary>
        protected Website? LoadWebsite()
        {
            var website = websites.GetWebsiteByID(Id);
            if (website == null) return null;

            WebsiteName = website.Name;
            SiteHost = website.Name;
            Tz = ResolveTimeZone(website.TimeZone);
            NowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tz);
            return website;
        }

        /// <summary>Resolves ?range= (or ?start=/&amp;end=) into a local [start, end) window. Call after LoadWebsite.</summary>
        protected void InitRange()
        {
            var (start, end, key) = ResolveRange(NowLocal);
            LocalStart = start;
            LocalEnd = end;
            SelectedRange = key;
            RangeStart = start;
            RangeEndInclusive = end.AddSeconds(-1);
            Bucket = ChooseBucket(start, end);
            RangeDescription = Describe(start, end, Bucket);
        }

        public DateTime ToUtc(DateTime local) =>
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Tz);

        public DateTime ToLocal(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Tz);

        /// <summary>Query values that reproduce the current website, range and bot setting.</summary>
        public Dictionary<string, string?> RangeQuery()
        {
            var q = new Dictionary<string, string?> { ["id"] = Id.ToString(), ["range"] = SelectedRange };
            if (SelectedRange == CustomRange)
            {
                q["start"] = RangeStart.ToString("yyyy-MM-dd");
                q["end"] = RangeEndInclusive.ToString("yyyy-MM-dd");
            }
            if (IncludeBots) q["bots"] = "1";
            return q;
        }

        /// <summary>Link to another report page, keeping the current range.</summary>
        public string RangeUrl(string page, params (string Key, string? Value)[] extra)
        {
            var q = RangeQuery();
            foreach (var (key, value) in extra)
            {
                if (!string.IsNullOrEmpty(value)) q[key] = value;
            }
            return QueryHelpers.AddQueryString(Url.Page(page) ?? page, q);
        }

        public RangePickerModel Picker(params (string Key, string Value)[] hidden) =>
            new(Id, SelectedRange, RangeStart, RangeEndInclusive, IncludeBots,
                hidden.Where(h => !string.IsNullOrEmpty(h.Value)).ToDictionary(h => h.Key, h => h.Value));

        // ---------- Range logic ----------

        private (DateTime Start, DateTime EndExclusive, string Key) ResolveRange(DateTime nowLocal)
        {
            var today = nowLocal.Date;
            var tomorrow = today.AddDays(1);
            var thisMonday = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var key = Range?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(key))
            {
                key = (Start.HasValue || End.HasValue) ? CustomRange : DefaultRange;
            }

            switch (key)
            {
                case "today": return (today, tomorrow, key);
                case "yesterday": return (today.AddDays(-1), today, key);
                case "last24h":
                    {
                        var currentHour = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, nowLocal.Hour, 0, 0);
                        return (currentHour.AddHours(-23), currentHour.AddHours(1), key);
                    }
                case "last7": return (today.AddDays(-6), tomorrow, key);
                case "thisweek": return (thisMonday, tomorrow, key);
                case "lastweek": return (thisMonday.AddDays(-7), thisMonday, key);
                case "last30": return (today.AddDays(-29), tomorrow, key);
                case "thismonth": return (monthStart, tomorrow, key);
                case "lastmonth": return (monthStart.AddMonths(-1), monthStart, key);
                case "last90": return (today.AddDays(-89), tomorrow, key);
                case CustomRange:
                    {
                        var s = Start?.Date ?? today.AddDays(-13);
                        var e = End?.Date ?? today;
                        if (s > e) (s, e) = (e, s);
                        return (s, e.AddDays(1), CustomRange);
                    }
                default:
                    return (today.AddDays(-13), tomorrow, DefaultRange);
            }
        }

        protected static ReportDateRangeCustomType ChooseBucket(DateTime start, DateTime endExclusive)
        {
            var span = endExclusive - start;
            if (span <= TimeSpan.FromHours(48)) return ReportDateRangeCustomType.Day;
            if (span <= TimeSpan.FromDays(62)) return ReportDateRangeCustomType.Days;
            return ReportDateRangeCustomType.Months;
        }

        protected static string Describe(DateTime start, DateTime endExclusive, ReportDateRangeCustomType bucket)
        {
            var endInclusive = endExclusive.AddSeconds(-1);
            if (bucket == ReportDateRangeCustomType.Day && (start.TimeOfDay != TimeSpan.Zero || endExclusive.TimeOfDay != TimeSpan.Zero))
            {
                return $"{start:d MMM yyyy, h:mm tt} – {endExclusive:d MMM yyyy, h:mm tt}";
            }
            if (start.Date == endInclusive.Date)
            {
                return start.ToString("ddd, d MMM yyyy");
            }
            return $"{start:d MMM yyyy} – {endInclusive:d MMM yyyy}";
        }

        private static TimeZoneInfo ResolveTimeZone(string? id)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id)) return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch { }
            return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        }
    }
}
