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
      <div className="mb-2 text-[10px] font-semibold uppercase tracking-wider text-[#1d1d1f]/50">
        Internal note
      </div>
      <div
        className="rounded-lg p-2.5 text-xs text-[#1d1d1f]"
        style={{ background: "#f5f5f7", fontSize: 12 }}
      >
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
                  ? "bg-[#1d1d1f] text-white"
                  : "border border-black/[0.1] text-[#1d1d1f]/60"
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
