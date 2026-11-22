# Tilted.Result

A compact `Result` type for .NET 11 built on the C# 15 [union types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union) feature. Success and error become an explicit, closed union, so `switch` expressions are exhaustive and neither case can be forgotten.

> Project: `Tilted.Result` · Namespace: `Tilted.Results`

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
| `Tap(onSuccess)` / `Tap(onSuccess, onError)` | Runs a side effect; returns the same result. |
| `TapError(onError)` | Runs a side effect on error only. |
| `Ensure(predicate, error)` / `Ensure(predicate, errorFactory)` | Fails a success that does not satisfy `predicate`. |
| `MapAsync(map)` / `BindAsync(bind)` | `ValueTask`-based `Map` / `Bind`. |
| `Equals` / `GetHashCode` / `==` / `!=` | Value equality based on `Value`. |
| `ToString()` | `Success(...)`, `Error(...)`, or `Empty`. |

### Factories

```csharp
Result.Ok<TValue, TError>(TValue value);
Result.Fail<TValue, TError>(TError error);
Result.Fail<TValue>(Exception exception);                    // TError = Exception

Result.Try<TValue>(Func<TValue> factory);                    // TError = Exception
Result.Try<TValue, TError>(Func<TValue> factory,
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
var saved = await profile.BindAsync(p => SaveAsync(p));
```

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
  - Throwing members: `Match`, `Map`, `MapError`, `Bind`, `Tap`, `TapError`, `Ensure`, `MapAsync`, `BindAsync`, `Unwrap`, `UnwrapError` throw `InvalidOperationException`.
  - Safe members: `IsSuccess`, `IsError`, `HasValue`, `TryGetValue`, `TryGetError`, `GetValueOrDefault`, `GetErrorOrDefault`, `Value`, `ToString` never throw.

## Limitations

- **`TValue` and `TError` must be distinct types.** The union discriminates by runtime type, so `Result<int, int>` is ambiguous and does not compile (`CS0457`).
- **Overlapping types are not discriminated.** With `Result<object, string>`, a `string` satisfies both cases and `Match` picks the success branch first. Keep `TValue` and `TError` disjoint, including base/derived pairs such as `Result<Exception, MyException>`.
- **Value types are boxed.** A `TValue` such as `int` is stored as a boxed `object` inside the union.
- **No structural tagging.** The case is identified by its type, not by a dedicated tag or enum.
- **No analyzers.** Misuse of overlapping types is not rejected at compile time.

## Build

```bash
dotnet build Tilted.Result.slnx
dotnet pack Tilted.Result/Tilted.Result.csproj -c Release
```

## Releasing

Publishing uses NuGet Trusted Publishing (OIDC); no long-lived API key is stored in the repository.

1. On nuget.org, open **Trusted Publishing** and add a policy: owner = your GitHub account or organization, repository = `Tilted.Result`, workflow file = `publish.yml`.
2. Set `user` in `.github/workflows/publish.yml` to your nuget.org username.
3. Push a version tag:

```bash
git tag v0.1.0
git push origin v0.1.0
```

The workflow packs with the tag version and pushes both `.nupkg` and `.snupkg` to nuget.org.

## Solution layout

```text
Tilted.Result.slnx
Tilted.Result/
  Tilted.Result.csproj
  Result.cs            Result<TValue, TError> union type
  Result.Factory.cs    Ok / Fail / Try factories
README.md
```
