# MicroCommerce — Task Completion Checklist

After completing any backend (C#) task:
1. Ensure no compiler warnings (`TreatWarningsAsErrors` is on)
2. Run `dotnet test src/MicroCommerce.ApiService.Tests` — all tests must pass
3. Check nullable reference types are handled
4. Verify EF migrations are added if schema changed (per-feature DbContext)
5. Ensure new entities use `Guid.CreateVersion7()` for IDs
6. Confirm strongly typed IDs use Vogen `[ValueObject<Guid>]`

After completing any frontend (TypeScript/React) task:
1. Run `cd src/MicroCommerce.Web && npm run lint`
2. Run `cd src/MicroCommerce.Web && npm run format`
3. Verify no TypeScript errors (strict mode)
4. Ensure new components use kebab-case filenames, PascalCase component names
5. Check `"use client"` is only added when truly needed

For full-stack tasks:
- Run both backend tests and frontend lint/format
- Consider running E2E tests if critical paths are affected
