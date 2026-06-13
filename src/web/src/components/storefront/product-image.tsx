import Image from "next/image";
import { cn } from "@/lib/utils";

// Design palette tones (hifi-styles): deterministic per-category gradient when
// a product has no photos yet, so the grid still reads as the Tiles design.
const TONES: Record<string, string> = {
  Vessels: "from-[#C96F4A] to-[#B45A38]",
  Tableware: "from-[#A9B49A] to-[#8A9B7C]",
  Drinkware: "from-[#DCB9AC] to-[#C99A8A]",
};
const FALLBACK_TONE = "from-[#E8E2D5] to-[#D9CFBC]";

export function ProductImage({
  photoUrl,
  category,
  name,
  className,
  sizes,
}: {
  photoUrl: string | undefined;
  category: string;
  name: string;
  className?: string;
  sizes?: string;
}) {
  if (photoUrl) {
    return (
      <div
        className={cn(
          "relative overflow-hidden rounded-lg bg-muted",
          className,
        )}
      >
        <Image
          src={photoUrl}
          alt={name}
          fill
          sizes={sizes ?? "25vw"}
          className="object-cover"
        />
      </div>
    );
  }
  return (
    <div
      aria-label={name}
      role="img"
      className={cn(
        "rounded-lg bg-gradient-to-br",
        TONES[category] ?? FALLBACK_TONE,
        className,
      )}
    />
  );
}
