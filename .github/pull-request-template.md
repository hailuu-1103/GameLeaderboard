## Change

Describe the behavior and reason for this change.

## Verification

- [ ] Acceptance criteria are satisfied.
- [ ] Existing behavior remains compatible.
- [ ] Edge and failure cases were considered.
- [ ] Business rules remain in Domain.
- [ ] Controllers contain no business or persistence logic.
- [ ] No forbidden project or namespace dependency was introduced.
- [ ] New public types are intentionally public.
- [ ] Expected failures use explicit application results.
- [ ] Unexpected failures remain sanitized.
- [ ] Tests are deterministic and independent of execution order.
- [ ] CancellationToken is forwarded through I/O boundaries.
- [ ] Warnings were fixed or suppressions were explicitly justified.
- [ ] dotnet format --verify-no-changes passes.
- [ ] dotnet build --warnaserror passes.
- [ ] dotnet test passes.