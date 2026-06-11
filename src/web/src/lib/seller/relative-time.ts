// Machine-timezone-independent relative-time helpers, all anchored to the
// America/Los_Angeles civil calendar so labels render identically regardless of
// the host machine's local zone.

const TZ = "America/Los_Angeles";

const MINUTE_MS = 60_000;
const HOUR_MS = 60 * MINUTE_MS;
const DAY_MS = 24 * HOUR_MS;

const TIME_FMT = new Intl.DateTimeFormat("en-US", {
  timeZone: TZ,
  hour: "numeric",
  minute: "2-digit",
  hour12: true,
});

const CIVIL_FMT = new Intl.DateTimeFormat("en-US", {
  timeZone: TZ,
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
});

/** "2:14 PM" / "11:08 AM" / "9:41 AM" in Pacific time. */
export function formatClockTime(d: Date): string {
  // Intl emits a NARROW NO-BREAK SPACE (U+202F) before AM/PM in some ICU
  // builds; normalize to a regular space so the label matches verbatim copy.
  return TIME_FMT.format(d).replace(/ /g, " ");
}

/** Pacific civil-calendar day index (days since epoch in the LA zone). */
function civilDayIndex(d: Date): number {
  const parts = CIVIL_FMT.formatToParts(d);
  const get = (type: string) =>
    Number(parts.find((p) => p.type === type)?.value ?? "0");
  // Treat the Pacific civil date as a UTC midnight so day arithmetic is exact.
  return Math.floor(
    Date.UTC(get("year"), get("month") - 1, get("day")) / DAY_MS,
  );
}

/** Whole Pacific calendar days between two instants (now − then). */
export function civilDayDiff(then: Date, now: Date): number {
  return civilDayIndex(now) - civilDayIndex(then);
}

/** Whole hours elapsed (floored) between two instants. */
export function elapsedHours(then: Date, now: Date): number {
  return Math.floor((now.getTime() - then.getTime()) / HOUR_MS);
}

/** Whole days elapsed (floored) between two instants. */
export function elapsedDays(then: Date, now: Date): number {
  return Math.floor((now.getTime() - then.getTime()) / DAY_MS);
}

/** Whole minutes elapsed (floored) between two instants. */
export function elapsedMinutes(then: Date, now: Date): number {
  return Math.floor((now.getTime() - then.getTime()) / MINUTE_MS);
}
