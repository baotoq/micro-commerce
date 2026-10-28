import { getAccessToken, requirePlatformOperator } from "@/lib/session";
import { listOperatorMerchants } from "@/lib/accounts";
import { MerchantList } from "./merchant-list";

export default async function MerchantsPage() {
  await requirePlatformOperator();
  const merchants = await listOperatorMerchants((await getAccessToken())!);

  return (
    <section>
      <h1 className="mb-6 text-2xl font-semibold">Merchants</h1>
      <MerchantList merchants={merchants} />
    </section>
  );
}
