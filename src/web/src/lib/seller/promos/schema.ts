import { z } from "zod";

const baseFields = {
  code: z.string().trim().min(1).max(40),
  description: z.string().trim().min(1).max(200),
  minOrderAmount: z.coerce.number().nonnegative().optional(),
  startsAt: z.string().datetime().optional(),
  endsAt: z.string().datetime().optional(),
  activateImmediately: z
    .union([z.boolean(), z.string()])
    .transform((v) => v === true || v === "true")
    .default(false),
};

export const promotionInputSchema = z
  .discriminatedUnion("kind", [
    z.object({
      kind: z.literal("percentage"),
      percentValue: z.coerce.number().int().min(1).max(100),
      ...baseFields,
    }),
    z.object({
      kind: z.literal("fixed"),
      fixedAmount: z.coerce.number().positive(),
      ...baseFields,
    }),
  ])
  .refine(
    (data) => {
      if (data.startsAt && data.endsAt) {
        return new Date(data.startsAt) < new Date(data.endsAt);
      }
      return true;
    },
    { message: "startsAt must be before endsAt", path: ["endsAt"] },
  );

export type PromotionFormInput = z.input<typeof promotionInputSchema>;
export type PromotionFormOutput = z.output<typeof promotionInputSchema>;
