import { Eye } from "lucide-react";
import { connection } from "next/server";
import { EmailComposerAudience } from "@/components/seller/marketing/email-composer-audience";
import { EmailComposerContent } from "@/components/seller/marketing/email-composer-content";
import { EmailComposerSchedule } from "@/components/seller/marketing/email-composer-schedule";
import { EmailPreview } from "@/components/seller/marketing/email-preview";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { getMarketingDraft } from "@/lib/seller/marketing/data";

export default async function MarketingPage() {
  await connection();
  const draft = await getMarketingDraft();
  return (
    <div className="flex h-screen min-w-0 flex-col overflow-hidden">
      <SellerTopbar
        title="Email recent buyers"
        subtitle="Marketing · drafted Tuesday"
        actions={
          <>
            <button
              type="button"
              className="inline-flex items-center rounded-lg border border-black/20 px-3.5 py-1.5 text-[13px] font-medium text-foreground hover:bg-black/[0.04]"
            >
              Save draft
            </button>
            <button
              type="button"
              className="inline-flex items-center gap-1.5 rounded-lg border border-black/20 px-3.5 py-1.5 text-[13px] font-medium text-foreground hover:bg-black/[0.04]"
            >
              <Eye className="size-3.5" aria-hidden />
              Send test
            </button>
            <button
              type="button"
              className="inline-flex items-center rounded-lg bg-foreground px-3.5 py-1.5 text-[13px] font-medium text-white hover:bg-foreground/90"
            >
              Schedule send
            </button>
          </>
        }
      />
      <div className="flex flex-1 overflow-hidden">
        {/* Left — composer */}
        <div
          className="flex-1 overflow-auto border-r border-black/[0.06]"
          style={{ padding: "24px 28px" }}
        >
          <EmailComposerAudience draft={draft} />
          <EmailComposerContent draft={draft} />
          <EmailComposerSchedule draft={draft} />
        </div>

        {/* Right — preview */}
        <div
          className="overflow-auto"
          style={{
            width: 480,
            padding: "24px 28px",
            background: "var(--canvas-parchment)",
          }}
        >
          <EmailPreview draft={draft} />
        </div>
      </div>
    </div>
  );
}
