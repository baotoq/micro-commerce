import type { OrderInboxStatus, StatusTone } from "@/lib/seller/orders/types";
import {
  civilDayDiff,
  elapsedHours,
  elapsedMinutes,
  formatClockTime,
} from "@/lib/seller/relative-time";

/**
 * Inbox "placed" label, anchored to the Pacific civil calendar:
 *   today     → "Today · 2:14 PM"
 *   yesterday → "Yesterday"
 *   2–6 days  → "N days ago"
 *   7–13 days → "1 week ago"
 *   ≥14 days  → "N weeks ago"
 */
export function formatPlacedLabel(placedAt: Date, now: Date): string {
  const days = civilDayDiff(placedAt, now);
  if (days <= 0) return `Today · ${formatClockTime(placedAt)}`;
  if (days === 1) return "Yesterday";
  if (days < 7) return `${days} days ago`;
  const weeks = Math.floor(days / 7);
  return weeks === 1 ? "1 week ago" : `${weeks} weeks ago`;
}

/**
 * Compact age chip for an inbox row: "2h" while same Pacific day, "1d"/"7d"
 * once a calendar boundary has been crossed.
 */
export function formatAge(placedAt: Date, now: Date): string {
  const days = civilDayDiff(placedAt, now);
  if (days >= 1) return `${days}d`;
  return `${elapsedHours(placedAt, now)}h`;
}

/**
 * Generic "ago" formatter used by the order timeline. The timeline anchors each
 * entry relative to the order's PlacedAt, so callers pass PlacedAt as `now`.
 *   <60m → "52m ago"
 *   <24h → "2h ago"
 *   else → "Nd ago"
 */
export function formatRelative(occurredAt: Date, now: Date): string {
  const mins = elapsedMinutes(occurredAt, now);
  if (mins < 60) return `${mins}m ago`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `${hours}h ago`;
  return `${Math.floor(hours / 24)}d ago`;
}

const STATUS_DISPLAY: Record<
  string,
  { label: OrderInboxStatus; tone: StatusTone }
> = {
  new: { label: "New", tone: "warn" },
  packed: { label: "Packed", tone: "mute" },
  shipped: { label: "Shipped", tone: "mute" },
  delivered: { label: "Delivered", tone: "good" },
  "refund-requested": { label: "Refund req.", tone: "bad" },
  cancelled: { label: "Cancelled", tone: "mute" },
};

/** Maps a backend wire status key to its display label + tone. */
export function statusToDisplay(wireKey: string): {
  label: OrderInboxStatus;
  tone: StatusTone;
} {
  return STATUS_DISPLAY[wireKey] ?? { label: "New", tone: "warn" };
}

/** "Sasha Leblanc" → "Sasha L." — for inbox table rows. */
export function shortName(fullName: string): string {
  const parts = fullName.trim().split(/\s+/).filter(Boolean);
  const first = parts[0];
  if (first === undefined) return "";
  const last = parts.length > 1 ? parts[parts.length - 1] : undefined;
  return last ? `${first} ${last[0]}.` : first;
}

/** "Sasha Leblanc" → "Sasha L" — for the detail panel header (no dot). */
export function shortNamePanel(fullName: string): string {
  const parts = fullName.trim().split(/\s+/).filter(Boolean);
  const first = parts[0];
  if (first === undefined) return "";
  const last = parts.length > 1 ? parts[parts.length - 1] : undefined;
  return last ? `${first} ${last[0]}` : first;
}
