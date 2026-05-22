const NAV_LINKS: { label: string; href: string }[] = [
  { label: "Discover", href: "/discover" },
  { label: "Shops", href: "/shops" },
  { label: "Journal", href: "/journal" },
  { label: "For makers", href: "/sell" },
];

export function MarketingTop({ active = "" }: { active?: string }) {
  return (
    <div
      className="flex items-center border-b border-black/[0.06] bg-white"
      style={{ padding: "14px 32px", gap: 28 }}
    >
      <span className="font-semibold text-foreground" style={{ fontSize: 20 }}>
        micro.
      </span>
      <div className="flex" style={{ gap: 28 }}>
        {NAV_LINKS.map(({ label, href }) => (
          <a
            key={label}
            href={href}
            style={{
              fontSize: 12.5,
              color:
                label === active
                  ? "var(--foreground)"
                  : "color-mix(in oklch, var(--foreground) 60%, transparent)",
              fontWeight: 400,
              textDecoration: "none",
            }}
          >
            {label}
          </a>
        ))}
      </div>
      <div className="flex-1" />
      <a
        href="/signin"
        className="rounded-full border border-black/20 px-4 py-1.5 text-sm font-medium text-foreground hover:bg-black/5"
        style={{ textDecoration: "none" }}
      >
        Sign in
      </a>
      <button
        type="button"
        className="rounded-full bg-primary px-4 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90"
        style={{ border: "none", cursor: "pointer" }}
      >
        Sell on Micro
      </button>
    </div>
  );
}
