// web/src/lib/seller/application/data.ts
import type {
  LaunchTask,
  SellerApplication,
  SetupStep,
} from "@/lib/seller/application/types";

const APPLICATION: SellerApplication = {
  shopName: "Mira Studio",
  domain: "mira-studio.micro.shop",
  available: true,
  category: "Ceramics",
  categories: ["Ceramics", "Bakery", "Textiles", "Jewelry", "Vintage", "Other"],
  stepsLeft: 6,
};
export function getApplication(): SellerApplication {
  return APPLICATION;
}

const SETUP_STEPS: SetupStep[] = [
  { label: "Shop name", status: "done" },
  { label: "Location & payouts", status: "active" },
  { label: "Brand", status: "pending" },
  { label: "First listing", status: "pending" },
  { label: "Shipping", status: "pending" },
  { label: "Review", status: "pending" },
];
export function getSetupSteps(): SetupStep[] {
  return SETUP_STEPS;
}

const LAUNCH_CHECKLIST: LaunchTask[] = [
  { label: "Claim shop name", subject: "alex-studio · 2 days ago", done: true },
  { label: "Add payout method", subject: "Bank · ending 4421", done: true },
  {
    label: "Publish first listing",
    subject: "Persimmon vase · just now",
    done: true,
  },
  {
    label: "Add 2 more listings",
    subject: "Most shops launch with 5+",
    done: false,
    hint: "Recommended",
  },
  {
    label: "Set shipping rates",
    subject: "US · Intl · Local pickup",
    done: false,
  },
  {
    label: "Share with 3 friends",
    subject: "Average shop gets first sale in 4 days",
    done: false,
  },
];
export function getLaunchChecklist(): LaunchTask[] {
  return LAUNCH_CHECKLIST;
}
