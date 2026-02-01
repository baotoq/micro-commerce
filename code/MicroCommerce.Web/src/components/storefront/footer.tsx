import Link from 'next/link';
import { Separator } from '@/components/ui/separator';

export function Footer() {
  return (
    <footer className="border-t border-zinc-200/80 bg-zinc-50">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-between">
          <p className="text-xs text-zinc-400">
            Built with Next.js &amp; .NET
          </p>
          <div className="flex items-center gap-4">
            <Link
              href="/admin"
              className="text-xs text-zinc-400 transition-colors hover:text-zinc-600"
            >
              Admin
            </Link>
            <Separator orientation="vertical" className="h-3" />
            <a
              href="#"
              className="text-xs text-zinc-400 transition-colors hover:text-zinc-600"
            >
              GitHub
            </a>
          </div>
        </div>
        <div className="mt-4 text-center sm:text-left">
          <p className="text-xs text-zinc-300">
            &copy; {new Date().getFullYear()} MicroCommerce. All rights reserved.
          </p>
        </div>
      </div>
    </footer>
  );
}
