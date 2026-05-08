import Link from "next/link";
import { Icon } from "@/components/icon";
import { ProductImage, Stars } from "@/components/primitives";
import { ShopperTopbar } from "@/components/shopper-topbar";
import { money } from "@/lib/money";

export default async function ProductPage({
  params,
}: {
  params: Promise<{ slug: string }>;
}) {
  const { slug } = await params;
  void slug;

  return (
    <div style={{ minHeight: "100vh", background: "var(--paper)" }}>
      <ShopperTopbar cartCount={2} current="shop" />

      <div
        style={{ maxWidth: "1280px", margin: "0 auto", padding: "20px 28px" }}
      >
        {/* Breadcrumb */}
        <div className="hf-crumb" style={{ marginBottom: 14 }}>
          <Link href="/">Shop</Link>
          <span className="hf-crumb-sep">/</span>
          <Link href="/shop">Vessels</Link>
          <span className="hf-crumb-sep">/</span>
          <span style={{ color: "var(--ink)" }}>Persimmon vase</span>
        </div>

        {/* 3-column product grid */}
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "60px 1fr 380px",
            gap: 18,
            height: 460,
          }}
        >
          {/* Thumb rail */}
          <div className="hf-col hf-gap-2">
            {(["clay", "rose", "rust", "bone"] as const).map((t, i) => (
              <div
                key={t}
                className={`hf-img hf-img-${t}`}
                style={{
                  height: 60,
                  borderRadius: 8,
                  border:
                    i === 0
                      ? "1.5px solid var(--ink)"
                      : "1px solid var(--line)",
                }}
              />
            ))}
          </div>

          {/* Hero image */}
          <div
            className="hf-relative hf-overflow-hidden"
            style={{ borderRadius: "var(--r-lg)" }}
          >
            <ProductImage tone="clay" h={460} r="var(--r-lg)" />
            <div
              style={{
                position: "absolute",
                top: 14,
                left: 14,
                padding: "4px 10px",
                borderRadius: 999,
                background: "rgba(255,255,255,0.92)",
                fontSize: 10.5,
                fontWeight: 600,
                color: "var(--ink)",
              }}
            >
              NEW · Spring &apos;26
            </div>
            <div
              style={{
                position: "absolute",
                bottom: 14,
                left: 14,
                display: "flex",
                gap: 6,
              }}
            >
              {[1, 2, 3, 4, 5].map((i) => (
                <span
                  key={i}
                  style={{
                    width: 6,
                    height: 6,
                    borderRadius: 999,
                    background: i === 1 ? "var(--ink)" : "rgba(0,0,0,0.25)",
                  }}
                />
              ))}
            </div>
            <button
              className="hf-icon-btn"
              type="button"
              style={{
                position: "absolute",
                top: 14,
                right: 14,
                background: "rgba(255,255,255,0.92)",
              }}
            >
              <Icon n="heart" s={14} />
            </button>
          </div>

          {/* Info column */}
          <div className="hf-col" style={{ paddingLeft: 8 }}>
            <span className="hf-eyebrow">Mira Studio · Oakland</span>
            <h1
              className="hf-display"
              style={{ fontSize: 36, marginTop: 6, lineHeight: 1 }}
            >
              Persimmon <i>vase</i>
            </h1>
            <div
              className="hf-flex hf-items-center hf-gap-2"
              style={{ marginTop: 8 }}
            >
              <Stars n={5} size={12} />
              <span className="hf-small hf-muted">4.9 · 38 reviews</span>
            </div>
            <div
              className="hf-flex hf-items-baseline hf-gap-2"
              style={{ marginTop: 14 }}
            >
              <span className="hf-display hf-num" style={{ fontSize: 26 }}>
                {money(86)}
              </span>
              <span className="hf-small hf-muted">+ tax</span>
            </div>

            {/* Color picker */}
            <div style={{ marginTop: 18 }}>
              <div
                className="hf-flex hf-between"
                style={{ marginBottom: 8, alignItems: "baseline" }}
              >
                <span className="hf-tiny hf-muted">
                  COLOR · <span style={{ color: "var(--ink)" }}>Persimmon</span>
                </span>
                <span className="hf-tiny hf-muted">4 of 4</span>
              </div>
              <div className="hf-flex hf-gap-2">
                {(["clay", "rose", "rust", "bone"] as const).map((t, i) => (
                  <div
                    key={t}
                    className={`hf-img hf-img-${t}`}
                    style={{
                      width: 30,
                      height: 30,
                      borderRadius: 999,
                      border:
                        i === 0
                          ? "2px solid var(--ink)"
                          : "1px solid var(--line-2)",
                      boxShadow: i === 0 ? "0 0 0 2px var(--paper)" : "none",
                    }}
                  />
                ))}
              </div>
            </div>

            {/* Size picker */}
            <div style={{ marginTop: 14 }}>
              <div className="hf-tiny hf-muted" style={{ marginBottom: 8 }}>
                SIZE
              </div>
              <div className="hf-flex hf-gap-2">
                {[
                  { l: "Small", s: "18cm" },
                  { l: "Medium", s: "26cm", on: true },
                  { l: "Large", s: "34cm", dis: true },
                ].map((o) => (
                  <div
                    key={o.l}
                    className="hf-flex hf-col hf-items-center"
                    style={{
                      flex: 1,
                      padding: "8px 0",
                      borderRadius: 8,
                      border: o.on
                        ? "1.5px solid var(--ink)"
                        : "1px solid var(--line-2)",
                      opacity: o.dis ? 0.4 : 1,
                      background: o.on ? "var(--paper-2)" : "transparent",
                      position: "relative",
                    }}
                  >
                    <div style={{ fontSize: 12, fontWeight: 600 }}>{o.l}</div>
                    <div className="hf-tiny hf-muted">{o.s}</div>
                    {o.dis && (
                      <div
                        className="hf-tiny"
                        style={{
                          position: "absolute",
                          top: 4,
                          right: 6,
                          color: "var(--ink-4)",
                        }}
                      >
                        —
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </div>

            {/* CTA row */}
            <div className="hf-flex hf-gap-2" style={{ marginTop: 18 }}>
              <div
                className="hf-flex hf-items-center"
                style={{
                  height: 44,
                  padding: "0 6px",
                  border: "1px solid var(--line-2)",
                  borderRadius: 999,
                }}
              >
                <button className="hf-icon-btn" type="button">
                  <Icon n="minus" s={13} />
                </button>
                <span
                  className="hf-num"
                  style={{
                    width: 18,
                    textAlign: "center",
                    fontSize: 13,
                    fontWeight: 600,
                  }}
                >
                  1
                </span>
                <button className="hf-icon-btn" type="button">
                  <Icon n="plus" s={13} />
                </button>
              </div>
              <button
                className="hf-btn hf-btn-primary hf-btn-lg"
                type="button"
                style={{ flex: 1 }}
              >
                Add to bag · {money(86)}
              </button>
            </div>

            {/* Trust lines */}
            <div
              className="hf-flex hf-items-center hf-gap-2 hf-small hf-muted"
              style={{ marginTop: 12 }}
            >
              <Icon n="truck" s={13} />
              <span>Free local delivery · ships in 3–5 days</span>
            </div>
            <div
              className="hf-flex hf-items-center hf-gap-2 hf-small hf-muted"
              style={{ marginTop: 4 }}
            >
              <Icon n="refresh" s={13} />
              <span>14-day returns · made one at a time</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
