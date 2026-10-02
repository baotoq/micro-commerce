import Link from "next/link";
import { signIn, signOut } from "@/auth";
import { Button } from "@/components/ui/button";
import { getCurrentAccount } from "@/lib/session";

export async function SiteHeader() {
  const me = await getCurrentAccount();

  return (
    <header className="flex items-center justify-between border-b px-6 py-3">
      <Link href="/" className="font-semibold">
        MicroCommerce
      </Link>
      {me ? (
        <div className="flex items-center gap-4">
          <Link href={me.merchant ? "/merchant" : "/open-shop"} className="text-sm hover:underline">
            {me.merchant ? "Merchant Centre" : "Open your Shop"}
          </Link>
          <form
            className="flex items-center gap-3"
            action={async () => {
              "use server";
              await signOut({ redirectTo: "/" });
            }}
          >
            <span className="text-sm">{me.displayName}</span>
            <Button type="submit" variant="outline" size="sm">
              Sign out
            </Button>
          </form>
        </div>
      ) : (
        <form
          action={async () => {
            "use server";
            await signIn("keycloak");
          }}
        >
          <Button type="submit" size="sm">
            Sign in
          </Button>
        </form>
      )}
    </header>
  );
}
