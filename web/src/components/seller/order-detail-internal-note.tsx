export function OrderDetailInternalNote({
  note,
  tags,
  activeTags,
}: {
  note: string;
  tags: string[];
  activeTags: string[];
}) {
  return (
    <div className="rounded-xl border border-black/[0.06] bg-white p-4">
      <div className="mb-2 text-[10px] font-semibold uppercase tracking-wider text-muted-foreground">
        Internal note
      </div>
      <div className="rounded-lg bg-canvas-parchment p-2.5 text-[12px] text-foreground">
        {note}
      </div>
      <div className="mt-2.5 flex flex-wrap gap-1">
        {tags.map((t) => {
          const on = activeTags.includes(t);
          return (
            <span
              key={t}
              className={`inline-flex items-center rounded-full px-2.5 py-1 text-[11px] font-medium ${
                on
                  ? "bg-foreground text-white"
                  : "border border-black/[0.1] text-muted-foreground"
              }`}
            >
              {t}
            </span>
          );
        })}
      </div>
    </div>
  );
}
