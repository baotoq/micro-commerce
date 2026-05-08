import Link from "next/link";
import { Icon } from "@/components/icon";
import { ProductImage } from "@/components/primitives";
import { ShopperTopbar } from "@/components/shopper-topbar";
import { money } from "@/lib/money";

const products = [
  {
    tone: "clay",
    title: "Persimmon vase",
    price: 86,
    badge: "New",
    slug: "persimmon-vase",
  },
  { tone: "sage", title: "Forest bowl, lg.", price: 64, slug: "forest-bowl" },
  {
    tone: "bone",
    title: "Cream tumbler — set of 2",
    price: 48,
    slug: "cream-tumbler-set",
  },
  {
    tone: "rose",
    title: "Soft hand vessel",
    price: 92,
    badge: "2 left",
    slug: "soft-hand-vessel",
  },
  { tone: "cobalt", title: "Indigo carafe", price: 110, slug: "indigo-carafe" },
  {
    tone: "cream",
    title: "Bone dinner plate",
    price: 38,
    slug: "bone-dinner-plate",
  },
  { tone: "rust", title: "Rust mug, Nº 04", price: 32, slug: "rust-mug" },
  { tone: "moss", title: "Moss saucer set", price: 44, slug: "moss-saucer" },
] as const;

const filterChips = ["All", "Vessels", "Tableware", "Drinkware", "Limited"];

export default function Home() {
  return (
    <div
      style={{
        minHeight: "100vh",
        background: "var(--paper)",
        fontFamily: "var(--sans)",
      }}
    >
      <ShopperTopbar cartCount={2} current="shop" />

      {/* Hero — full-bleed */}
      <div style={{ position: "relative", height: 320, overflow: "hidden" }}>
        <div
          className="hf-img hf-img-clay"
          style={{
            position: "absolute",
            inset: 0,
            borderRadius: 0,
            height: "100%",
          }}
        />
        <div
          style={{
            position: "absolute",
            inset: 0,
            background:
              "linear-gradient(90deg, rgba(0,0,0,0.45), rgba(0,0,0,0.05) 60%)",
          }}
        />
        <div
          style={{
            position: "relative",
            maxWidth: "1280px",
            margin: "0 auto",
            padding: "0 28px",
            height: "100%",
            display: "flex",
            flexDirection: "column",
            justifyContent: "center",
          }}
        >
          <div style={{ color: "white", maxWidth: 480 }}>
            <div
              className="hf-eyebrow"
              style={{ color: "rgba(255,255,255,0.8)" }}
            >
              Spring &apos;26 · Vessels
            </div>
            <h1
              className="hf-display"
              style={{
                fontSize: 52,
                color: "white",
                marginTop: 8,
                lineHeight: 1,
              }}
            >
              Hand-thrown for slow
              <br />
              <i>mornings.</i>
            </h1>
            <div className="hf-flex hf-gap-2" style={{ marginTop: 18 }}>
              <Link
                href="/shop"
                className="hf-btn hf-btn-lg"
                style={{
                  background: "white",
                  color: "var(--ink)",
                  display: "inline-flex",
                  alignItems: "center",
                  gap: 6,
                  textDecoration: "none",
                }}
              >
                Shop the drop
                <Icon n="arrowR" s={13} />
              </Link>
              <Link
                href="/about"
                className="hf-btn hf-btn-lg"
                style={{
                  background: "transparent",
                  border: "1px solid rgba(255,255,255,0.5)",
                  color: "white",
                  textDecoration: "none",
                }}
              >
                The studio
              </Link>
            </div>
          </div>
        </div>
      </div>

      {/* Filter bar */}
      <div
        style={{
          borderBottom: "1px solid var(--line)",
          maxWidth: "100%",
        }}
      >
        <div
          className="hf-flex hf-items-center"
          style={{
            maxWidth: "1280px",
            margin: "0 auto",
            padding: "14px 28px",
            justifyContent: "space-between",
          }}
        >
          <div className="hf-flex hf-gap-2" style={{ flexWrap: "wrap" }}>
            {filterChips.map((chip, i) => (
              <span
                key={chip}
                className={`hf-chip${i === 0 ? " hf-chip-on" : ""}`}
              >
                {chip}
              </span>
            ))}
          </div>
          <div className="hf-flex hf-items-center hf-gap-3">
            <span className="hf-muted" style={{ fontSize: 12.5 }}>
              42 pieces
            </span>
            <span
              className="hf-flex hf-items-center hf-gap-1"
              style={{ color: "var(--ink-2)", fontWeight: 500, fontSize: 12.5 }}
            >
              Sort: Newest
              <Icon n="chevD" s={11} />
            </span>
          </div>
        </div>
      </div>

      {/* Product grid */}
      <div
        style={{
          maxWidth: "1280px",
          margin: "0 auto",
          padding: "24px 28px 48px",
        }}
      >
        <div
          style={{
            display: "grid",
            gridTemplateColumns: "repeat(4, 1fr)",
            gap: 20,
          }}
          className="product-grid"
        >
          {products.map((p) => (
            <Link
              key={p.slug}
              href={`/product/${p.slug}`}
              style={{ textDecoration: "none", color: "inherit" }}
              className="hover:opacity-90 transition-opacity"
            >
              <ProductImage
                tone={p.tone}
                h={240}
                badge={"badge" in p ? p.badge : undefined}
              />
              <div
                className="hf-flex"
                style={{
                  marginTop: 10,
                  justifyContent: "space-between",
                  alignItems: "flex-start",
                  gap: 8,
                }}
              >
                <span className="hf-h4">{p.title}</span>
                <span className="hf-h4 hf-num" style={{ flexShrink: 0 }}>
                  {money(p.price)}
                </span>
              </div>
              <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>
                Stoneware · 4 colors
              </div>
            </Link>
          ))}
        </div>
      </div>

      <style>{`
        @media (max-width: 1024px) {
          .product-grid {
            grid-template-columns: repeat(2, 1fr) !important;
          }
        }
        @media (max-width: 640px) {
          .product-grid {
            grid-template-columns: repeat(1, 1fr) !important;
          }
        }
      `}</style>
    </div>
  );
}
