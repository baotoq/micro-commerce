import { Toaster } from 'sonner';

import { Header } from '@/components/storefront/header';
import { Footer } from '@/components/storefront/footer';
import { QueryProvider } from '@/components/providers/query-provider';

export default function StorefrontLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <QueryProvider>
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex-1">{children}</main>
        <Footer />
        <Toaster position="top-right" />
      </div>
    </QueryProvider>
  );
}
