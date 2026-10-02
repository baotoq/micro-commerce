import Link from "next/link";
import { auth, signIn, signOut } from "@/auth";
import { Button } from "@/components/ui/button";
import { getMe, type Me } from "@/lib/accounts";

async function currentAccount(): Promise<Me | null> {
  const session = await auth();
  if (!session?.accessToken || session.error) return null;
  // An expired or rejected token just means "not signed in" here; the user can sign in again.
  return getMe(session.accessToken).catch(() => null);
}

export async function SiteHeader() {
  const me = await currentAccount();

  return (
    <header className="flex items-center justify-between border-b px-6 py-3">
      <Link href="/" className="font-semibold">
        MicroCommerce
      </Link>
      {me ? (
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
