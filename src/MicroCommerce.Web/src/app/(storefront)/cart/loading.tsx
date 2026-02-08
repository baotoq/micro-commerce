import { Skeleton } from "@/components/ui/skeleton";

export default function CartLoading() {
  return (
    <div>
      <Skeleton className="mb-8 h-8 w-20" />
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          {Array.from({ length: 3 }).map((_, i) => (
            <div
              key={i}
              className="flex gap-4 rounded-lg border border-zinc-200 p-4"
            >
              <Skeleton className="size-16 shrink-0 rounded-md" />
              <div className="flex flex-1 flex-col gap-2">
                <Skeleton className="h-4 w-2/3" />
                <Skeleton className="h-4 w-20" />
              </div>
              <Skeleton className="h-8 w-24" />
            </div>
          ))}
        </div>
        <div>
          <Skeleton className="h-48 rounded-xl" />
        </div>
      </div>
    </div>
  );
}
