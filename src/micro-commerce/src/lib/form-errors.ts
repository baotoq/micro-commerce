import { ApiError } from "@/lib/api";

// Maps API validation failures onto the form's fields (by their camelCased, dotted names);
// failures for anything else, and non-validation errors, become a form-level error.
export function toSubmitErrors(error: unknown, fieldNames: readonly string[]) {
  if (!(error instanceof ApiError)) {
    return { form: "Something went wrong. Please try again." };
  }

  const failures = error.problem?.errors ?? [];
  const fieldFailures = failures.filter((e) => fieldNames.includes(e.name));
  const otherFailures = failures.filter((e) => !fieldNames.includes(e.name));

  return {
    form:
      otherFailures.map((e) => e.reason).join(" ") ||
      (fieldFailures.length === 0 ? error.message : undefined),
    fields: Object.fromEntries(fieldFailures.map((e) => [e.name, { message: e.reason }])),
  };
}
