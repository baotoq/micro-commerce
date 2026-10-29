import { getCart } from "@/lib/cart";
import { getAccessToken, getCurrentAccount } from "@/lib/session";
import { SiteHeaderNavigation } from "./site-header-navigation";

export async function SiteHeader() {
  const [me, accessToken] = await Promise.all([getCurrentAccount(), getAccessToken()]);
  const cart = me && accessToken ? await getCart(accessToken).catch(() => null) : null;

  return (
    <header className="border-b px-3 py-3 sm:px-6">
      <div className="mx-auto flex max-w-7xl items-center gap-2">
        <SiteHeaderNavigation me={me} cartLineCount={cart?.lines.length ?? 0} />
      </div>
    </header>
  );
}
