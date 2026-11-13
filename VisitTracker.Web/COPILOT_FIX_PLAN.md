# WebStats (VisitTracker) – Fix Plan for Copilot

Put this file in the repo root as `COPILOT_FIX_PLAN.md`. Then paste the prompt below into Copilot Chat (Agent mode), one phase at a time.

---

## Prompt to paste into Copilot

```
You are working on WebStats, a website visit-tracking service.
Solution: VTracker-Sol.sln (.NET 9, ASP.NET Core Razor Pages + MVC controllers, EF Core 8, SQL Server).
Projects: VisitTracker.Web, VisitTracker.DataContext, VisitTracker.Models.
Tracker script source: VisitTracker.Web/wwwroot/vt.ts (compiled to vt.js, served by RVController.getjs).

Read COPILOT_FIX_PLAN.md in the repo root. It contains the full plan, the rules, and acceptance criteria.

Do ONLY Phase <N> now. For each task in that phase:
1. Briefly state what you will change and in which files.
2. Make the change.
3. Check it against the acceptance criteria in the plan.
When the phase is done, stop and give me a summary: files changed, SQL scripts added, anything you were unsure about, and how I can test it manually.

Follow the "Rules" section strictly. In particular: do NOT add login/authentication, do NOT rename existing
tracking endpoints or query parameters, and do NOT use EF migrations – write idempotent SQL scripts in /db instead.
If something in the plan does not match the actual code, tell me instead of guessing.
```

Replace `<N>` with 0, 1, 2… Review and test each phase before starting the next.

---

## Rules (apply to every phase)

1. **No login/auth work.** Authentication is intentionally not implemented yet. Don't add `[Authorize]` to pages and don't build login screens.
2. **Backward compatibility with installed sites.** Customer sites already embed the `vtinit` snippet that loads `rv/getjs/{id}`. Keep these unchanged:
   - Route names `rv/fr`, `rv/index`, `rv/rc`, `rv/wb`, `rv/wf`, `rv/sb`, `rv/getjs`
   - Existing query parameter names and meanings
   - New data goes in **new optional** parameters. The server must still accept requests from an old cached `vt.js`.
3. **Database changes = idempotent SQL scripts** in `/db/NNN_description.sql` (guarded with `IF COL_LENGTH(...) IS NULL` / `IF NOT EXISTS (SELECT 1 FROM sys.indexes ...)`). Also update the EF model classes and `OnModelCreating` to match. No EF migrations. Check the real column names in the database or model before writing SQL. FK columns are expected to be `WebsiteID`, `VisitID`, `WebpageID`, `VisitPageID`; verify.
4. **`vt.ts` is the source of truth.** After editing it, regenerate `vt.js`. Keep it plain ES5-compatible output, with no modules or imports, because it is injected as a classic script. Keep the `var vt_guid = '';` line exactly as it is, because `getjs` string-replaces it.
5. **Tracking endpoints must never throw.** Every `rv/*` action returns the transparent pixel (or empty content for `getjs`) even on bad input. Validate with `Uri.TryCreate` and `Guid.TryParse`, catch and log.
6. Store all times in **UTC**. Convert to the site's time zone only for display.
7. Keep changes small and focused per task. Don't reformat unrelated code.
8. Put reusable logic (URL normalization, referrer grouping, browser/bot detection) in small static helper classes.
9. **Do not create a unit test project** or add test frameworks. Verification is manual (see the checklist at the end).

---

## Phase 0 – Housekeeping (no behaviour change)

| # | Task | Files | Acceptance criteria |
|---|------|-------|---------------------|
| 0.1 | Move Serilog file output out of `wwwroot`. Use `logs/applog-.txt` relative to the content root, not the web root. Add `logs/` and `wwwroot/logs/` to `.gitignore`. | `appsettings*.json`, `.gitignore` | No log file can be reached via a URL. The app still writes logs. |
| 0.2 | Move secrets out of `appsettings*.json` (connection string, `IP2LocationKey`, `Jwt:Key`). Use User Secrets locally and environment variables in production. Keep empty placeholders in the JSON files. | `appsettings*.json`, `VisitTracker.Web.csproj` (UserSecretsId) | The repo contains no secret values. Add a README note on how to set them. |
| 0.3 | `IPLocationWorker`: use `IHttpClientFactory` (named client with a 5 s timeout) instead of `new HttpClient()` per call. Register `IPLocationWorker`, `WebsiteManager`, `VisitManager`, `WebpageManager` in DI as scoped services instead of `new`-ing them in controllers and pages. | `IPLocationWorker.cs`, `Program.cs`, controllers, pages, components | Builds. Behaviour is unchanged. |

---

## Phase 1 – Database indexes (biggest performance win)

Every pulse, click and blur currently calls `GetVisitByCC`, which scans the whole `Visit` table because `ClientCookie` has no index. The logs show `SELECT COUNT(*) FROM Visit` timing out after 30 s.

| # | Task | Acceptance criteria |
|---|------|---------------------|
| 1.1 | `/db/001_indexes.sql`, with these indexes:<br>• `Visit(ClientCookie)` – unique if the data allows; check for duplicates first and report them<br>• `Visit(WebsiteID, DateCreated) INCLUDE (LastVisitID, BrowserName, Referer, ScreenWidth, ScreenHeight)`<br>• `VisitPage(VisitID, ID DESC) INCLUDE (WebpageID)`<br>• `VisitPage(WebpageID, DateCreated)`<br>• `VisitActivity(VisitID)` and `VisitActivity(VisitPageID) INCLUDE (Activity, MouseClickX, MouseClickY, ClickTagName, ClickTagId)`<br>• `Webpage(WebsiteID) INCLUDE (Path, QueryString)` | The script is idempotent. Each index has a comment explaining which query it serves. |
| 1.2 | `Webpage.Path` is `nvarchar(1024)`, which is too wide for an index key. Add a persisted computed column `PathHash AS CAST(HASHBYTES('SHA2_256', LOWER(Path) + '?' + LOWER(QueryString)) AS binary(32)) PERSISTED`, index `(WebsiteID, PathHash)`, and make `WebpageManager.GetWebpage` filter on the hash and then on Path. | `GetWebpage` uses an index seek, not a scan. |
| 1.3 | Fix in-memory counting: `WebsiteManager.GetWebsiteCount`, `GetWebsites` and `WebpageManager.GetWebpageCount` call `.ToList()` before filtering. Filter in SQL instead. | No `ToList()` before `Where`/`Count` in managers. |
| 1.4 | Home page (`WebsiteList.razor`, the count components): replace the per-website `COUNT` queries with one grouped query per table (`GROUP BY WebsiteID`). Add `IMemoryCache` with a 5-minute expiry for the home page totals. | The home page runs ≤ 4 SQL queries in total and loads in under 1 s on the current data. |

---

## Phase 2 – Make tracking endpoints cheap and safe (`RVController`)

| # | Task | Acceptance criteria |
|---|------|---------------------|
| 2.1 | Add a private helper `TryResolve(Guid cc, string path, out Visit, out Webpage, out VisitPage)` and use it in `index`, `rc`, `wb`, `wf`, `sb`, removing the duplicated code. Use `Uri.TryCreate`. On any failure, return the pixel. | No unhandled exception from any `rv/*` action for garbage input (checked manually). |
| 2.2 | Update `LastPingDate` with `ExecuteUpdate` (single UPDATE statement) instead of loading the entity, setting `EntityState.Modified` on the whole entity and saving. Do the same for `VisitPage.LastPingDate`. | A pulse does at most 3 SQL round trips (find visit + current page, update visit, update page). |
| 2.3 | Response headers: pixel responses send `Cache-Control: no-store`. `getjs` sends `Cache-Control: no-store` and `Content-Type: application/javascript`, because each load embeds a fresh GUID. | Verified in the browser network tab. |
| 2.4 | `getjs` only serves the script when the `Referer` host matches the site, and returns empty otherwise. Keep that check, but also accept the `Origin` header, and log (at Information level, rate-limited) when both are missing so silent tracking failures are visible. | Behaviour is the same for normal sites. Missing-referer cases are logged. |
| 2.5 | Add basic rate limiting on `rv/*` with ASP.NET Core `RateLimiter` (for example, 60 requests/minute per IP + cc, fixed window). | Excess requests get the pixel back but are not written to the DB. |

---

## Phase 3 – Tracker script fixes (`vt.ts` → `vt.js`)

| # | Task | Acceptance criteria |
|---|------|---------------------|
| 3.1 | **Fix the swapped blur/focus endpoints.** `windowBlur()` currently calls `wf/` and `windowFocus()` calls `wb/`. Swap them so blur → `wb/` and focus → `wf/`. | Blurring the window creates a `WindowBlur` (2) activity. |
| 3.2 | **Encoding:** use `encodeURIComponent` (not `encodeURI`) for every query value: `path`, `referer`, `tagid`, `b`, etc. Remove the double encoding of `referer`, and make sure the server decodes exactly once (remove the extra `HttpUtility.UrlDecode` in the reports if it becomes redundant). | A page URL like `/p?utm_source=a&utm_campaign=b#x` arrives at the server intact. |
| 3.3 | **Scroll tracking:** replace the `mousewheel`/`DOMMouseScroll` listener with a passive, throttled (250 ms) `scroll` listener. Track `maxScrollPct` (0–100) and `docHeight`. Keep the existing `sb/` call when the user reaches the bottom. Send `ms=<maxScrollPct>&dh=<docHeight>` as new optional params on each pulse. | Scroll-to-bottom fires on phones (touch), keyboard and the scrollbar. Pulses carry `ms` and `dh`. |
| 3.4 | **Pulse frequency:** change from 5 s to 15 s. Pause the pulse when `document.visibilityState === 'hidden'` and resume when visible. On `pagehide`, send one final pulse. | No pulses from background tabs. Server load drops about 3×. |
| 3.5 | **Returning visitors:** the permanent ID currently expires after 720 minutes (12 h), so almost nobody counts as returning. Add a persistent visitor ID `vid` in localStorage that never expires (generate it client-side with `crypto.randomUUID()`, with a fallback). Send `vid` on `fr`. Keep sending `occ` as before for compatibility. | The same browser returning after 2 days is detected as returning (see 4.4). |
| 3.6 | **Browser detection:** check in this order: `HeadlessChrome` → "Headless Chrome", `Edg/` → Edge, `OPR/` → Opera, `SamsungBrowser` → Samsung Internet, `CriOS` → Chrome (iOS), `FxiOS` → Firefox (iOS), then Chrome, Safari, Firefox. Don't send the raw user agent from JS: the server reads it from the request header (4.3). | Modern Edge reports as Edge. |
| 3.7 | **Clicks:** in the document click handler, find `event.target.closest('a')`. If found, send new optional params `href` (the link's href, max 500 chars) and `txt` (trimmed innerText, max 100 chars). For non-links, still send the tag and id as today. | Clicking a `tel:` or `wa.me` link records its href. |
| 3.8 | Regenerate `vt.js` from `vt.ts` and confirm the old `vt.js` divergence is gone. Add `wwwroot/tracker-test.html`: a long page with a `tel:` link, a `wa.me` link, a form, and an anchor with a query string, for manual testing on localhost. | `vt.js` is generated from the current `vt.ts`. The test page works. |

---

## Phase 4 – Server-side data quality

| # | Task | Acceptance criteria |
|---|------|---------------------|
| 4.1 | `/db/002_tracking_columns.sql`:<br>• `Visit`: `VisitorId uniqueidentifier NULL` (indexed with WebsiteID), `UserAgent nvarchar(400) NULL`, `IsBot bit NOT NULL DEFAULT 0`, `UtmSource/UtmMedium/UtmCampaign nvarchar(100) NULL`<br>• `VisitPage`: `MaxScrollPct tinyint NULL`, `DocumentHeight int NULL`<br>• `VisitActivity`: `ClickHref nvarchar(500) NULL`, `ClickText nvarchar(100) NULL`<br>• `Website`: `TimeZone varchar(64) NOT NULL DEFAULT 'India Standard Time'`<br>Update the model classes to match. | The script is idempotent. The app builds and runs against the updated DB. |
| 4.2 | **URL normalization** (`UrlNormalizer` static class): lowercase the path, strip the trailing slash (except root), and remove tracking params (`utm_*`, `fbclid`, `gclid`, `msclkid`, `yclid`, `mc_cid`, `mc_eid`, `_ga`, `ref`, `igshid`). Sort the remaining params. Use it in `fr` and in `TryResolve` before the `Webpage` lookup. Capture the UTM values onto the `Visit` in `fr`. | `/a/x?utm_source=fb` and `/a/x` map to the same Webpage. UTM values are saved. |
| 4.3 | Store the new pulse/click params: `ms`/`dh` → `VisitPage.MaxScrollPct` (keep the max) and `DocumentHeight`. `href`/`txt` → `VisitActivity`. Save `Request.Headers.UserAgent` (truncated) on the Visit in `fr`. | Values appear in the DB after using `tracker-test.html`. |
| 4.4 | **New vs returning:** in `fr`, if `vid` is present, set `VisitorId`. A visit is returning if an earlier Visit exists for the same `WebsiteID` + `VisitorId`. Keep setting `LastVisitID` (to that earlier visit's ID) so the existing reports work unchanged. | Returning visits show up in the report chart. |
| 4.5 | **Bot detection** (`BotDetector` static class). Set `IsBot = true` in `fr` when any of these is true:<br>• the UA contains `HeadlessChrome`, `bot`, `crawler`, `spider`, `Lighthouse`, `PhantomJS`, `Puppeteer`, `Playwright`<br>• the UA is empty<br>• `IsProxy` from IP2Location is true<br>• the screen is exactly 1600×1600 or 800×600 **and** the UA is missing common mobile/desktop tokens<br>Also add `/db/003_backfill_isbot.sql`, which flags historical visits using screen size and `IsProxy` (UA isn't available for old rows). | A headless/bot user agent gets `IsBot = 1`; a normal browser doesn't. The backfill script is idempotent. |
| 4.6 | **Historical blur/focus data:** add `/db/004_swap_old_blur_focus.sql`, which swaps `Activity` 2↔3 for rows created **before** a `@DeployDateUtc` variable I will set. Wrap it in a transaction and print the row counts. Don't run it automatically. | The script exists, is parameterized, and is safe to run once. |

---

## Phase 5 – Reports

| # | Task | Acceptance criteria |
|---|------|---------------------|
| 5.1 | **Chart legend bug:** in `Report.cshtml` and `PageReport.cshtml`, Chartist colours the series All/New/Return as series a/b/c (red, light red, yellow), but the legend says All = cyan, Return = red, New = yellow. Give each series an explicit `className` and CSS colour, and make the legend match exactly. | The legend colours match the lines. |
| 5.2 | Exclude `IsBot` visits from all report numbers by default. Add an "Include bots" toggle (query string `bots=1`) and show "X bot visits hidden". | The counts drop accordingly. The toggle works. |
| 5.3 | Move aggregation to SQL: daily/hourly counts, new vs returning, referrers, screen sizes, browsers and top pages via `GroupBy` queries, instead of loading every Visit into memory and looping. Remove the pages × visitPages loop and the `SingleOrDefault`-inside-loop pattern. | The report for rockying.com (16 days) loads in under 1 s. No query returns full Visit entities for the whole range. |
| 5.4 | **Time zone:** bucket days and hours in `Website.TimeZone` (IST by default). Convert the start/end inputs from local time to UTC for the query. Fix the hourly boundary double count (use `>= start && < end`). | A visit at 01:00 IST on 5 Oct counts on 5 Oct, not 4 Oct. |
| 5.5 | **Referrer grouping** (`ReferrerGrouper` static class): map `www.google.*`, `search.google.com`, `com.google.android.googlequicksearchbox`, `android-app://com.google…` → "Google"; `*.bing.com` → "Bing"; `l.facebook.com`, `m.facebook.com`, `lm.facebook.com` → "Facebook"; `l.instagram.com` → "Instagram"; `t.co` → "Twitter/X"; empty → "Direct / none". Parse safely (malformed referrers must not throw). Show the grouped view with expandable raw hosts. | Google variants appear as one row. A bad referrer string never breaks the report. |
| 5.6 | Browser chart: add Edge (from 3.6), Samsung Internet and Headless (when bots are included). Match on the stored `BrowserName` prefix, case-insensitive. | Edge is no longer 0. "Other" shrinks. |
| 5.7 | Mark today's partial bucket on the chart (dashed line or label "today (partial)"). | Visible on the chart. |

---

## Manual test checklist (after each phase)

- [ ] Open `tracker-test.html` locally, scroll to the bottom on desktop **and** in mobile emulation, click the `tel:` and `wa.me` links, switch tabs and come back.
- [ ] Check the new rows in `Visit`, `VisitPage` and `VisitActivity`: correct activity types, href saved, MaxScrollPct set, no pulses while the tab is hidden.
- [ ] Close the browser, reopen it the next day (or fake the localStorage time), and visit again → counted as returning.
- [ ] Send garbage to `rv/index?cc=abc&path=notaurl` → returns the pixel, no 500, warning logged.
- [ ] Home page and rockying.com report load quickly. No SQL timeouts in the logs.
- [ ] `dotnet build` passes with no new warnings.

## Out of scope for now

Login/authentication, uptime monitoring, goals/leads UI, heatmap rendering, WhatsApp reports. These come after the data is accurate.
