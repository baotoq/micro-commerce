"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { errorMessage } from "@/lib/api";
import { reinstateMerchant, suspendMerchant, type OperatorMerchant } from "@/lib/accounts";

export function MerchantList({ merchants }: { merchants: OperatorMerchant[] }) {
  const router = useRouter();
  const [pendingId, setPendingId] = useState<string>();
  const [error, setError] = useState<string>();

  async function changeStatus(merchant: OperatorMerchant) {
    setPendingId(merchant.id);
    setError(undefined);
    try {
      if (merchant.status === "Suspended") await reinstateMerchant(merchant.id);
      else await suspendMerchant(merchant.id);
      router.refresh();
    } catch (e) {
      setError(errorMessage(e));
    } finally {
      setPendingId(undefined);
    }
  }

  if (merchants.length === 0) return <p>No Merchants yet.</p>;

  return (
    <div className="space-y-4">
      {error && (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      )}
      <ul className="divide-y rounded-md border">
        {merchants.map((merchant) => (
          <li key={merchant.id} className="flex flex-col gap-3 p-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="min-w-0">
              <h2 className="font-medium">{merchant.shopName}</h2>
              <p className="break-all text-xs text-muted-foreground">{merchant.id}</p>
              <p className="text-sm text-muted-foreground">
                {merchant.status} · Opened <time dateTime={merchant.createdAt}>{merchant.createdAt.slice(0, 10)}</time>
              </p>
            </div>
            <Button
              variant={merchant.status === "Suspended" ? "outline" : "destructive"}
              size="sm"
              aria-label={`${merchant.status === "Suspended" ? "Reinstate" : "Suspend"} ${merchant.shopName}`}
              disabled={pendingId !== undefined}
              onClick={() => void changeStatus(merchant)}
            >
              {pendingId === merchant.id ? "Saving…" : merchant.status === "Suspended" ? "Reinstate" : "Suspend"}
            </Button>
          </li>
        ))}
      </ul>
    </div>
  );
}
