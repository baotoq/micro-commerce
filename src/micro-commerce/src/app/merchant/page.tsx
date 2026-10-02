import { requireMerchant } from "@/lib/session";
import { ShopProfile } from "./shop-profile";

export default async function MerchantCentrePage() {
  const merchant = await requireMerchant();

  return <ShopProfile merchant={merchant} />;
}
