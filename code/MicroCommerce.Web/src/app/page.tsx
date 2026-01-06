import Image from "next/image";
import { AuthButton } from "@/components/auth/auth-button";
import { ApiTest } from "@/components/api-test";

export default function Home() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-50 font-sans dark:bg-black">
      <main className="flex min-h-screen w-full max-w-3xl flex-col gap-8 py-16 px-8 bg-white dark:bg-black sm:px-16">
        <div className="flex w-full items-center justify-between">
          <Image
            className="dark:invert"
            src="/next.svg"
            alt="Next.js logo"
            width={100}
            height={20}
            priority
          />
          <AuthButton />
        </div>

        <div className="flex flex-col gap-4">
          <h1 className="text-2xl font-semibold tracking-tight text-black dark:text-zinc-50">
            API Test
          </h1>
          <p className="text-zinc-600 dark:text-zinc-400">
            Test the protected .NET API endpoints with your Keycloak token.
          </p>
          <ApiTest />
        </div>
      </main>
    </div>
  );
}
