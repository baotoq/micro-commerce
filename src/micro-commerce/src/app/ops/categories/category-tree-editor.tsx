"use client";

import { useForm } from "@tanstack/react-form";
import { useRouter } from "next/navigation";
import { useId, useState } from "react";
import { Button } from "@/components/ui/button";
import { Field, FieldError, FieldLabel } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { ApiError } from "@/lib/api";
import {
  type CategoryNameInput,
  type CategoryNode,
  categoryNameSchema,
  createCategory,
  deleteCategory,
  renameCategory,
} from "@/lib/catalog";
import { toSubmitErrors } from "@/lib/form-errors";

// Lets a Platform Operator grow, rename and prune the Category tree. After each change the server
// component re-reads the tree, so the page always shows what the API has.
export function CategoryTreeEditor({ tree }: { tree: CategoryNode[] }) {
  return (
    <section>
      <h1 className="mb-6 text-2xl font-semibold">Categories</h1>
      <CategoryNameForm
        label="New top-level Category"
        submitLabel="Add Category"
        resetOnSuccess
        onSubmit={(input) => createCategory(input)}
      />
      {tree.length === 0 ? (
        <p className="mt-8 text-muted-foreground">No Categories yet.</p>
      ) : (
        <ul className="mt-8 rounded-lg border px-4 py-2">
          {tree.map((node) => (
            <CategoryItem key={node.id} node={node} />
          ))}
        </ul>
      )}
    </section>
  );
}

function CategoryItem({ node }: { node: CategoryNode }) {
  const router = useRouter();
  const [mode, setMode] = useState<"view" | "rename" | "addChild">("view");
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string>();

  async function handleDelete() {
    setIsDeleting(true);
    setDeleteError(undefined);
    try {
      await deleteCategory(node.id);
      router.refresh();
    } catch (error) {
      const reasons = error instanceof ApiError ? error.problem?.errors?.map((e) => e.reason) : undefined;
      setDeleteError(reasons?.join(" ") || "Couldn't delete this Category. Please try again.");
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <li className="py-1">
      {mode === "rename" ? (
        <CategoryNameForm
          label={`Rename ${node.name}`}
          submitLabel="Save"
          defaultName={node.name}
          onSubmit={(input) => renameCategory(node.id, input)}
          onDone={() => setMode("view")}
        />
      ) : (
        <div className="flex flex-wrap items-center justify-between gap-2">
          <span>{node.name}</span>
          <div className="flex gap-2">
            <Button variant="ghost" size="sm" aria-label={`Add a Category under ${node.name}`} onClick={() => setMode("addChild")}>
              Add child
            </Button>
            <Button variant="ghost" size="sm" aria-label={`Rename ${node.name}`} onClick={() => setMode("rename")}>
              Rename
            </Button>
            <Button
              variant="ghost"
              size="sm"
              aria-label={`Delete ${node.name}`}
              disabled={isDeleting}
              onClick={() => void handleDelete()}
            >
              Delete
            </Button>
          </div>
        </div>
      )}
      {deleteError && (
        <p role="alert" className="text-sm text-destructive">
          {deleteError}
        </p>
      )}
      {mode === "addChild" && (
        <div className="my-2 ml-6">
          <CategoryNameForm
            label={`New Category under ${node.name}`}
            submitLabel="Add"
            onSubmit={(input) => createCategory(input, node.id)}
            onDone={() => setMode("view")}
          />
        </div>
      )}
      {node.children.length > 0 && (
        <ul className="ml-3 border-l pl-4">
          {node.children.map((child) => (
            <CategoryItem key={child.id} node={child} />
          ))}
        </ul>
      )}
    </li>
  );
}

// One Category name field, used to add top-level and child Categories and to rename one.
// Passing onDone shows a Cancel button and is called after a successful submit or a cancel.
function CategoryNameForm({
  label,
  submitLabel,
  defaultName = "",
  resetOnSuccess = false,
  onSubmit,
  onDone,
}: {
  label: string;
  submitLabel: string;
  defaultName?: string;
  resetOnSuccess?: boolean;
  onSubmit: (input: CategoryNameInput) => Promise<unknown>;
  onDone?: () => void;
}) {
  const router = useRouter();
  const id = useId();
  const form = useForm({
    defaultValues: { name: defaultName },
    validators: {
      onChange: categoryNameSchema,
      onSubmitAsync: async ({ value }) => {
        try {
          await onSubmit(categoryNameSchema.parse(value));
          return null;
        } catch (error) {
          return toSubmitErrors(error, ["name"]);
        }
      },
    },
    onSubmit: ({ formApi }) => {
      if (resetOnSuccess) formApi.reset();
      onDone?.();
      router.refresh();
    },
  });

  return (
    <form
      className="flex flex-col gap-2"
      onSubmit={(e) => {
        e.preventDefault();
        void form.handleSubmit();
      }}
    >
      <form.Field name="name">
        {(field) => {
          const isInvalid = field.state.meta.isTouched && !field.state.meta.isValid;
          return (
            <Field data-invalid={isInvalid}>
              <FieldLabel htmlFor={id}>{label}</FieldLabel>
              <div className="flex gap-2">
                <Input
                  id={id}
                  name={field.name}
                  value={field.state.value}
                  onBlur={field.handleBlur}
                  onChange={(e) => field.handleChange(e.target.value)}
                  aria-invalid={isInvalid}
                  autoFocus={onDone !== undefined}
                />
                <form.Subscribe selector={(state) => state.isSubmitting}>
                  {(isSubmitting) => (
                    <>
                      <Button type="submit" disabled={isSubmitting}>
                        {isSubmitting ? "Saving…" : submitLabel}
                      </Button>
                      {onDone && (
                        <Button type="button" variant="outline" onClick={onDone} disabled={isSubmitting}>
                          Cancel
                        </Button>
                      )}
                    </>
                  )}
                </form.Subscribe>
              </div>
              {isInvalid && <FieldError errors={field.state.meta.errors} />}
            </Field>
          );
        }}
      </form.Field>
      <form.Subscribe selector={(state) => state.errorMap.onSubmit}>
        {(formError) => typeof formError === "string" && <FieldError>{formError}</FieldError>}
      </form.Subscribe>
    </form>
  );
}
