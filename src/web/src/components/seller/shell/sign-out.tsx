import { LogOut } from "lucide-react";
import { signOut } from "@/lib/auth/config";

/**
 * Sign-out control for the seller shell. Renders a server-action form so it
 * stays a Server Component — no client bundle, no onClick handler. Submitting
 * invokes the Auth.js `signOut` server action, which clears the session cookie
 * and redirects to the post-logout landing.
 */
export function SignOut() {
  return (
    <form
      action={async () => {
        "use server";
        await signOut({ redirectTo: "/" });
      }}
    >
      <button
        type="submit"
        aria-label="Sign out"
        className="inline-flex h-9 items-center gap-1.5 rounded-full px-3 text-xs font-medium text-foreground/70 hover:bg-black/[0.04] hover:text-foreground"
      >
        <LogOut className="size-4" aria-hidden />
        Sign out
      </button>
    </form>
  );
}
