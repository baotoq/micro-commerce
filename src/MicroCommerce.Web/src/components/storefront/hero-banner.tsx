import Link from "next/link";
import { Button } from "@/components/ui/button";

export function HeroBanner() {
  return (
    <section className="relative overflow-hidden bg-gradient-to-br from-[#EFF6FF] to-[#F0FDF4]">
      <div className="mx-auto flex max-w-7xl items-center gap-12 px-4 py-20 sm:px-6 lg:px-20 lg:py-0">
        {/* Left: Content */}
        <div className="flex flex-1 flex-col gap-6 py-16 lg:py-20">
          <span className="inline-flex w-fit rounded-full bg-accent px-3.5 py-1.5 text-[13px] font-medium text-accent-foreground">
            New Collection 2026
          </span>
          <h1 className="max-w-[500px] text-4xl font-extrabold tracking-tight text-foreground sm:text-5xl">
            Discover Your Style
          </h1>
          <p className="max-w-[480px] text-base leading-relaxed text-muted-foreground">
            Explore our curated collection of premium products. From everyday
            essentials to luxury finds, shop with confidence.
          </p>
          <div className="flex items-center gap-3">
            <Button
              asChild
              size="lg"
              className="h-12 px-7 text-base font-semibold"
            >
              <Link href="#products">Shop Now</Link>
            </Button>
            <Button
              asChild
              variant="outline"
              size="lg"
              className="h-12 px-7 text-base font-semibold"
            >
              <Link href="#products">Browse Catalog</Link>
            </Button>
          </div>
        </div>

        {/* Right: Hero image placeholder */}
        <div className="hidden lg:block">
          <div className="h-[340px] w-[500px] overflow-hidden rounded-xl bg-muted" />
        </div>
      </div>
    </section>
  );
}
