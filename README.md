# Tilted.Result

A compact `Result` type for .NET 11 built on the C# 15 [union types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union) feature. Success and error become an explicit, closed union, so `switch` expressions are exhaustive and neither case can be forgotten.

> Project: `Tilted.Result` · Namespace: `Tilted.Result`

## Requirements

| Requirement | Value |
| --- | --- |
| Target framework | `net11.0` |
| Language version | C# 15 (default for `net11.0`) |
| NuGet dependencies | none |

## Installation

```bash
dotnet add package Tilted.Result
```

The package is also mirrored to GitHub Packages: `https://nuget.pkg.github.com/tilted-develop/index.json`.

Build from source:

```bash
dotnet build Tilted.Result.slnx
```

## Quick start

```csharp
using Tilted.Results;

Result<int, string> ok = 42;       // implicit union conversion
Result<int, string> err = "boom";

string text = ok switch
{
    int value => $"ok: {value}",
    string error => $"error: {error}",
};

// text == "ok: 42"
```

## Core concepts

`Result<TValue, TError>` is a `readonly struct` and a real C# union type:

- `TValue` — the success case;
- `TError` — the error case; any type you choose (`string`, `enum`, a custom `record`, `Exception`, ...);
- the active case is determined by the **runtime type** of the stored value, not by a tag;
- implicit conversions exist from both `TValue` and `TError`;
- exhaustive `switch` requires no fallback branch.

## API

### `Result<TValue, TError>` members

| Member | Description |
| --- | --- |
| `Result(TValue)` / `Result(TError)` | Union constructors (power implicit conversions). Reject `null`. |
| `Value` | Boxed underlying value (`object?`), required by `IUnion`. |
| `IsSuccess` / `IsError` | Whether the result holds `TValue` / `TError`. |
| `HasValue` | `false` only for `default(Result<TValue, TError>)`. |
| `TryGetValue(out TValue)` | Extracts the success value; annotated `[MaybeNullWhen(false)]`. |
| `TryGetError(out TError)` | Extracts the error value; annotated `[MaybeNullWhen(false)]`. |
| `GetValueOrDefault(TValue)` | Success value or fallback. |
| `GetErrorOrDefault(TError)` | Error value or fallback. |
| `Unwrap()` / `UnwrapError()` | Value or `InvalidOperationException`. |
| `Match(onSuccess, onError)` | Folds the result into a single value. |
| `Map(map)` | Transforms success; propagates error. |
| `MapError(map)` | Transforms error; propagates success. |
| `Bind(bind)` | Chains `Result<TResult, TError>`; propagates error. |
| `Select(selector)` / `SelectMany(collectionSelector, resultSelector)` | LINQ aliases for `Map` / `Bind`; enable `from ? select` query syntax. |
| `Tap(onSuccess)` / `Tap(onSuccess, onError)` | Runs a side effect; returns the same result. |
| `TapError(onError)` | Runs a side effect on error only. |
| `Ensure(predicate, error)` / `Ensure(predicate, errorFactory)` | Fails a success that does not satisfy `predicate`. |
| `MapAsync(map)` / `BindAsync(bind)` | `ValueTask`-based `Map` / `Bind`. |
| `MapErrorAsync(map)` / `MatchAsync(onSuccess, onError)` | `ValueTask`-based `MapError` / `Match`. |
| `TapAsync(onSuccess)` / `TapAsync(onSuccess, onError)` / `TapErrorAsync(onError)` | `ValueTask`-based `Tap` / `TapError`. |
| `EnsureAsync(predicate, error)` / `EnsureAsync(predicate, errorFactory)` | `ValueTask`-based `Ensure`. |
| `Equals` / `GetHashCode` / `==` / `!=` | Value equality based on `Value`. |
| `ToString()` | `Success(...)`, `Error(...)`, or `Empty`. |
| `ResultAsyncExtensions` | `Map`/`Bind`/`MapError`/`Match`/`Tap`/`TapError`/`Ensure` for `ValueTask<Result<TValue, TError>>` and `Task<Result<TValue, TError>>` with synchronous continuations; `*Async` variants take asynchronous continuations. |
| `Result.Collect(...)` | Combines a sequence of results into one success list; returns the first error. |
| `Result.CollectAll(...)` | Combines a sequence, accumulating every error into `IReadOnlyList<TError>`. |
| `Result.Combine(...)` | Combines 2 to 8 results into a named tuple; returns the first error. |

### Factories

```csharp
Result.Ok<TValue, TError>(TValue value);
Result.Fail<TValue, TError>(TError error);
Result.Fail<TValue>(Exception exception);                    // TError = Exception

Result.Try<TValue>(Func<TValue> factory);                    // TError = Exception
Result.Try<TValue, TError>(Func<TValue> factory,
    Func<Exception, TError> onError);

Result.TryAsync<TValue>(Func<ValueTask<TValue>> factory);    // TError = Exception
Result.TryAsync<TValue, TError>(Func<ValueTask<TValue>> factory,
    Func<Exception, TError> onError);
```

## Usage

### Create and inspect

```csharp
var ok = Result.Ok<int, string>(42);
var err = Result.Fail<int, string>("boom");
Result<int, Exception> failed = Result.Fail<int>(new InvalidOperationException("x"));

if (ok.TryGetValue(out var value))
{
    Console.WriteLine(value); // 42
}

if (err.TryGetError(out var error))
{
    Console.WriteLine(error); // boom
}
```

### Transform

```csharp
var doubled = ok.Map(v => v * 2);                         // Result<int, string>
var otherError = err.MapError(e => e.Length);             // Result<int, int>
var chained = ok.Bind(v => v > 0
    ? Result.Ok<int, string>(v + 1)
    : Result.Fail<int, string>("negative"));               // Result<int, string>
var folded = ok.Match(v => $"ok: {v}", e => $"error: {e}");
```

`Map`, `MapError` and `Bind` propagate the other case unchanged.

### LINQ query syntax

`Select` and `SelectMany` are aliases for `Map` and `Bind`, so query expressions work on a `Result`:

```csharp
Result<int, string> a = 2, b = 3;

Result<int, string> sum =
    from x in a
    from y in b
    select x + y;                     // Ok(5)

Result<int, string> failed =
    from x in a
    from y in Result.Fail<int, string>("boom")
    select x + y;                     // Fail("boom")
```

All `from` clauses must use the same `TError`. `where`, `orderby`, `group` and `join` are not supported
(a single `Result` cannot drop a value without an error); use `Ensure` for guards instead.

### Side effects and guards

```csharp
var result = ok
    .Tap(v => Console.WriteLine($"got {v}"))
    .Ensure(v => v > 0, "must be positive")
    .Ensure(v => v % 2 == 0, v => $"must be even: {v}");
```

### Exception capture

```csharp
var parsed = Result.Try(() => int.Parse("42"));           // Result<int, Exception>

var mapped = Result.Try<int, string>(
    () => int.Parse("nope"),
    ex => ex.Message);                                     // Result<int, string>
```

### Async composition

```csharp
var user = await GetUserAsync(id);
var profile = await user.MapAsync(u => LoadProfileAsync(u));
var valid = await profile.EnsureAsync(p => p.Id > 0, "invalid id");
var saved = await valid.BindAsync(p => SaveAsync(p));
```

### Composing sync and async

`Result` exposes both synchronous (`Map`, `Bind`, `Ensure`, ...) and asynchronous (`MapAsync`,
`BindAsync`, `EnsureAsync`, ...) members, so a pipeline can mix both without explicit conversions:
the first asynchronous call returns `ValueTask<Result<TValue, TError>>`, and the extension methods in
`ResultAsyncExtensions` continue the chain from there. Base extension names (`Map`, `Bind`, ...) take
synchronous continuations, while the `*Async` variants take asynchronous ones.

```csharp
var outcome = await Result.Ok<int, string>(id)
    .Map(FindUser)                         // sync member
    .Bind(u => IsValid(u) ? Result.Ok<UserDto, string>(u) : Result.Fail<UserDto, string>("invalid"))
    .MapAsync(LoadProfileAsync)            // async member -> ValueTask<Result<...>>
    .Tap(profile => log.Information("profile {Id}", profile.Id)); // async-layer extension

var fromTask = await GetUserTask(id)       // Task<Result<UserDto, string>>
    .Ensure(u => u.IsActive, "inactive")   // async-layer extension, sync continuation
    .TapAsync(u => AuditAsync(u.Id));      // async-layer extension, async continuation
```

The extensions are thin wrappers over the members of `Result`, so a `default` result still throws `InvalidOperationException`.

### Real-world composition

**Checkout pipeline** - sync guards, async I/O, error projection and side effects in one chain:

```csharp
sealed record OrderError(string Code, string Message);

static ValueTask<Result<OrderConfirmation, OrderError>> CheckoutAsync(Cart cart)
{
    return Result.Ok<Cart, OrderError>(cart)
        .Ensure(c => c.Items.Any(), new OrderError("Empty", "Cart is empty"))          // sync guard
        .Map(c => Pricing.Total(c))                                                    // sync transform
        .BindAsync(total => Warehouse.TryReserveAsync(cart.Items, total))              // async bind
        .MapAsync(reservation => Payments.CaptureAsync(reservation))                   // async map
        .Bind(receipt => receipt.Total > 0
            ? Result.Ok<Receipt, OrderError>(receipt)
            : Result.Fail<Receipt, OrderError>(new("InvalidTotal", "Total must be positive")))
        .MapError(e => e with { Message = $"Checkout failed: {e.Message}" })           // error projection
        .Tap(r => Logger.Info("Paid {Total}", r.Total))                                // side effect
        .MapAsync(receipt => Inventory.FinalizeAsync(receipt));                        // async final step
}
```

**Parallel aggregation and validation** - `Result.Combine` loads independent sources concurrently,
while `Result.CollectAll` accumulates every validation error:

```csharp
static ValueTask<Result<Dashboard, Error>> BuildDashboardAsync(User user)
{
    return Result.Combine(
        LoadProfileAsync(user),     // ValueTask<Result<Profile, Error>>
        LoadOrdersAsync(user),      // ValueTask<Result<IReadOnlyList<Order>, Error>>
        LoadSettingsAsync(user))    // ValueTask<Result<Settings, Error>>
        .Map(t => new Dashboard(t.First, t.Second, t.Third))
        .TapAsync(d => ValueTask.CompletedTask);
}

sealed record ValidationError(string Field, string Message);

static Result<string, ValidationError> Required(string field, string? value) =>
    string.IsNullOrWhiteSpace(value)
        ? Result.Fail<string, ValidationError>(new(field, "Required"))
        : Result.Ok<string, ValidationError>(value);

static Result<UserDraft, IReadOnlyList<ValidationError>> Validate(UserDraft draft) =>
    Result.CollectAll(
        Required("Name",  draft.Name),
        Required("Email", draft.Email),
        MinLength("Password", draft.Password, 8))
        .Map(_ => draft);
```

The helpers (`Pricing`, `Warehouse`, `Payments`, `Inventory`, `LoadProfileAsync`, `MinLength`, ...) are illustrative.

### Pattern matching

```csharp
string Describe(Result<int, string> result) => result switch
{
    int value => $"ok: {value}",
    string error => $"error: {error}",
    null => "empty",
};
```

`switch` over all case types is exhaustive without a fallback branch. Add a `null` branch if the value may be `default`.

## State policy

- **`null` is rejected.** Constructors throw `ArgumentNullException` for a `null` value or error. Any result created through the public API holds either a success or an error.
- **The only empty state is `default(Result<TValue, TError>)`.**
  - Throwing members: `Match`, `Map`, `MapError`, `Bind`, `Select`, `SelectMany`, `Tap`, `TapError`, `Ensure`, `MapAsync`, `BindAsync`, `MapErrorAsync`, `MatchAsync`, `TapAsync`, `TapErrorAsync`, `EnsureAsync`, `Unwrap`, `UnwrapError` throw `InvalidOperationException`.
  - Safe members: `IsSuccess`, `IsError`, `HasValue`, `TryGetValue`, `TryGetError`, `GetValueOrDefault`, `GetErrorOrDefault`, `Value`, `ToString` never throw.

## Limitations

- **`TValue` and `TError` must be distinct types.** The union discriminates by runtime type, so `Result<int, int>` is ambiguous and does not compile (`CS0457`). Implicit conversions fail at compile time; the factory methods (`Ok`, `Fail`, `Try`) additionally reject equal type arguments at runtime.
- **Overlapping types are not discriminated.** With `Result<object, string>`, a `string` satisfies both cases and `Match` picks the success branch first. Keep `TValue` and `TError` disjoint, including base/derived pairs such as `Result<Exception, MyException>`.
- **Value types are boxed.** A `TValue` such as `int` is stored as a boxed `object` inside the union.
- **No structural tagging.** The case is identified by its type, not by a dedicated tag or enum.
- **Analyzer coverage is limited to concrete type arguments.** `TRESULT001`/`TRESULT002` do not analyze generic contexts where `TValue` or `TError` is an unbound type parameter.

## Analyzers

The package ships a Roslyn analyzer (`Tilted.Result.Analyzers`) that validates `Result<TValue, TError>` type arguments:

| ID | Severity | Description |
| --- | --- | --- |
| `TRESULT001` | Error | `TValue` and `TError` are the same type (`Result<int, int>`, `Result.Ok<string, string>(...)`). |
| `TRESULT002` | Warning | `TValue` and `TError` overlap by an implicit reference conversion (`Result<Exception, ApplicationException>`). |

The analyzer complements the compiler: `Result<int, int> value = 1;` already fails with `CS0457`, but explicitly typed factory calls such as `Result.Ok<int, int>(1)` compile, and `TRESULT001` reports them.

To make overlap warnings fail the build:

```xml
<PropertyGroup>
  <WarningsAsErrors>$(WarningsAsErrors);TRESULT002</WarningsAsErrors>
</PropertyGroup>
```

## Build

```bash
dotnet build Tilted.Result.slnx
dotnet pack Tilted.Result/Tilted.Result.csproj -c Release
```
