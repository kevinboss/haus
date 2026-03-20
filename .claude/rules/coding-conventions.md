# Coding Conventions

- File-scoped namespaces
- Primary constructors where they improve readability
- `var` when the type is obvious from context
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Keep methods short (~30 lines max), extract when exceeded
- Async methods suffixed with `Async`
- No `#region` blocks
- Constants over magic strings/numbers
- Use C# 14 `field` keyword in property accessors where it reduces boilerplate
