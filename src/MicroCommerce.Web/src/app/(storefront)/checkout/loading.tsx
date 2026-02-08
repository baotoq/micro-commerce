import { Skeleton } from "@/components/ui/skeleton";

export default function CheckoutLoading() {
  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <Skeleton className="mb-8 h-8 w-32" />
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          <Skeleton className="h-64 rounded-lg" />
          <Skeleton className="h-16 rounded-lg" />
        </div>
        <div>
          <Skeleton className="h-80 rounded-lg" />
        </div>
      </div>
    </div>
  );
}
