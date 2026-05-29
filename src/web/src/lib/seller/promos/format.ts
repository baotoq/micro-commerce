type FormatWhatInput = {
  kind: "percentage" | "fixed";
  percentValue?: number | null;
  fixedAmount?: number | null;
  description: string;
  minOrderAmount?: number | null;
};

export function formatPromoWhat(dto: FormatWhatInput): string {
  const kindTail =
    dto.kind === "percentage"
      ? `${dto.percentValue}% off`
      : `$${dto.fixedAmount} off`;

  const minSuffix =
    dto.minOrderAmount && dto.minOrderAmount > 0
      ? ` $${dto.minOrderAmount}+`
      : "";

  return `${kindTail} · ${dto.description}${minSuffix}`;
}

type FormatWindowInput = {
  startsAt?: string | null;
  endsAt?: string | null;
  status: string;
};

export function formatPromoWindow(dto: FormatWindowInput): string {
  if (dto.startsAt && dto.endsAt) {
    const fmt = (iso: string) => {
      const d = new Date(iso);
      return d.toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        timeZone: "UTC",
      });
    };
    return `${fmt(dto.startsAt)} → ${fmt(dto.endsAt)}`;
  }
  if (dto.status === "Active") return "Always";
  if (dto.status === "Draft") return "Drafted";
  return "Ended";
}
