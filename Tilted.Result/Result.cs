using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Tilted.Result;

/// <summary>
/// Represents the outcome of an operation: either a success value of type
/// <typeparamref name="TValue"/> or an error of type <typeparamref name="TError"/>.
/// </summary>
/// <remarks>
/// <para>
/// This is a C# 15 union type (see <see cref="UnionAttribute"/> and <see cref="IUnion"/>),
/// so it supports implicit conversions from both case types and exhaustive pattern
/// matching in <c>switch</c> expressions:
/// </para>
/// <code>
/// Result&lt;int, string&gt; result = 42;
/// string text = result switch
/// {
///     int value =&gt; $"ok: {value}",
///     string error =&gt; $"error: {error}",
/// };
/// </code>
/// <para>
/// <typeparamref name="TValue"/> and <typeparamref name="TError"/> must be distinct types,
/// because the union discriminates cases by runtime type rather than by a tag.
/// Using the same type for both (for example, <c>Result&lt;int, int&gt;</c>) is ambiguous
/// and is rejected by the compiler.
/// </para>
/// <para>
/// The constructors reject <c>null</c> values with an <see cref="ArgumentNullException"/>,
/// so an initialized result always holds either a success or an error value. The only way to
/// obtain a result without a value is <c>default(Result&lt;TValue, TError&gt;)</c>.
/// </para>
/// <para>
/// Members that must produce a value or run a side effect (<see cref="Match{TResult}"/>,
/// <see cref="Map{TResult}"/>, <see cref="MapError{TNewError}"/>, <see cref="Bind{TResult}"/>,
/// <c>Select</c>, <c>SelectMany</c>, <c>Tap</c>, <c>TapError</c>, <c>Ensure</c>, <c>MapAsync</c>, <c>BindAsync</c>,
/// <c>MapErrorAsync</c>, <c>MatchAsync</c>, <c>TapAsync</c>, <c>TapErrorAsync</c>, <c>EnsureAsync</c>,
/// <see cref="Unwrap"/>, <see cref="UnwrapError"/>) throw <see cref="InvalidOperationException"/>
/// on a <c>default</c> result. Safe members (<see cref="IsSuccess"/>, <see cref="IsError"/>,
/// <see cref="HasValue"/>, <see cref="TryGetValue"/>, <see cref="TryGetError"/>,
/// <see cref="GetValueOrDefault"/>, <see cref="GetErrorOrDefault"/>, <see cref="Value"/>,
/// and <see cref="ToString"/>) never throw.
/// </para>
/// </remarks>
/// <typeparam name="TValue">The type of the success value.</typeparam>
/// <typeparam name="TError">The user-defined type of the error value.</typeparam>
[Union]
public readonly partial struct Result<TValue, TError> : IUnion, IEquatable<Result<TValue, TError>>
{
    /// <summary>Creates a successful result that contains <paramref name="value"/>.</summary>
    /// <param name="value">The success value. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public Result(TValue value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a failed result that contains <paramref name="error"/>.</summary>
    /// <param name="error">The error value. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public Result(TError error)
    {
        if (error is null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        Value = error;
    }

    /// <summary>Gets the boxed underlying value of the union.</summary>
    public object? Value { get; }

    /// <summary>Gets a value indicating whether this result holds a success value.</summary>
    public bool IsSuccess => Value is TValue;

    /// <summary>Gets a value indicating whether this result holds an error value.</summary>
    public bool IsError => Value is TError;

    /// <summary>
    /// Gets a value indicating whether this result is initialized with any value.
    /// Only a <c>default</c> result has no value; the constructors never create a valueless result.
    /// </summary>
    public bool HasValue => Value is not null;

    /// <summary>Attempts to get the success value.</summary>
    /// <param name="value">The success value when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if this result holds a success value; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
    {
        if (Value is TValue success)
        {
            value = success;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>Attempts to get the error value.</summary>
    /// <param name="error">The error value when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if this result holds an error value; otherwise, <see langword="false"/>.</returns>
    public bool TryGetError([MaybeNullWhen(false)] out TError error)
    {
        if (Value is TError failure)
        {
            error = failure;
            return true;
        }

        error = default!;
        return false;
    }

    /// <summary>Returns the success value, or <paramref name="fallback"/> when this result holds an error.</summary>
    /// <param name="fallback">The value to return when this result holds an error.</param>
    public TValue GetValueOrDefault(TValue fallback) => Value is TValue success ? success : fallback;

    /// <summary>Returns the error value, or <paramref name="fallback"/> when this result is successful.</summary>
    /// <param name="fallback">The value to return when this result holds a success value.</param>
    public TError GetErrorOrDefault(TError fallback) => Value is TError failure ? failure : fallback;

    /// <summary>Returns the success value, or throws when this result holds an error or no value.</summary>
    /// <exception cref="InvalidOperationException">This result does not hold a success value.</exception>
    public TValue Unwrap() =>
        Value is TValue success
            ? success
            : throw new InvalidOperationException(
                $"Cannot unwrap a success value from a result that holds an error of type '{typeof(TError)}' or no value.");

    /// <summary>Returns the error value, or throws when this result holds a success value or no value.</summary>
    /// <exception cref="InvalidOperationException">This result does not hold an error value.</exception>
    public TError UnwrapError() =>
        Value is TError failure
            ? failure
            : throw new InvalidOperationException(
                $"Cannot unwrap an error from a result that holds a success of type '{typeof(TValue)}' or no value.");

    /// <summary>Projects this result into a single value by applying one of the supplied functions.</summary>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="onSuccess">The function to apply to a success value.</param>
    /// <param name="onError">The function to apply to an error value.</param>
    /// <exception cref="InvalidOperationException">This result does not hold any value.</exception>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        return Value switch
        {
            TValue success => onSuccess(success),
            TError failure => onError(failure),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Maps the success value using <paramref name="map"/>. Error values are propagated unchanged.
    /// </summary>
    /// <typeparam name="TResult">The type of the mapped success value.</typeparam>
    /// <param name="map">The transformation to apply to a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TResult, TError> Map<TResult>(Func<TValue, TResult> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return Value switch
        {
            TValue success => new Result<TResult, TError>(map(success)),
            TError failure => new Result<TResult, TError>(failure),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Maps the error value using <paramref name="map"/>. Success values are propagated unchanged.
    /// </summary>
    /// <typeparam name="TNewError">The type of the mapped error value.</typeparam>
    /// <param name="map">The transformation to apply to an error value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TValue, TNewError> MapError<TNewError>(Func<TError, TNewError> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return Value switch
        {
            TValue success => new Result<TValue, TNewError>(success),
            TError failure => new Result<TValue, TNewError>(map(failure)),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Binds the success value using <paramref name="bind"/>. Error values are propagated unchanged.
    /// </summary>
    /// <typeparam name="TResult">The type of the bound success value.</typeparam>
    /// <param name="bind">The continuation to apply to a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TResult, TError> Bind<TResult>(Func<TValue, Result<TResult, TError>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);

        return Value switch
        {
            TValue success => bind(success),
            TError failure => new Result<TResult, TError>(failure),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// LINQ projection alias for <see cref="Map{TResult}"/>; enables <c>select</c> in query expressions.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected success value.</typeparam>
    /// <param name="selector">The transformation to apply to a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TResult, TError> Select<TResult>(Func<TValue, TResult> selector) => Map(selector);

    /// <summary>
    /// LINQ binding alias for <see cref="Bind{TResult}"/>; enables a chained <c>from</c> in query expressions.
    /// </summary>
    /// <typeparam name="TResult">The type of the bound success value.</typeparam>
    /// <param name="selector">The continuation to apply to a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TResult, TError> SelectMany<TResult>(Func<TValue, Result<TResult, TError>> selector) => Bind(selector);

    /// <summary>
    /// Enables <c>from x in source from y in collectionSelector(x) select resultSelector(x, y)</c>
    /// in query expressions. The error of the nested result is propagated unchanged.
    /// </summary>
    /// <typeparam name="TCollection">The type of the intermediate success value.</typeparam>
    /// <typeparam name="TResult">The type of the projected success value.</typeparam>
    /// <param name="collectionSelector">Produces the next result from a success value.</param>
    /// <param name="resultSelector">Combines both success values into the projected value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TResult, TError> SelectMany<TCollection, TResult>(
        Func<TValue, Result<TCollection, TError>> collectionSelector,
        Func<TValue, TCollection, TResult> resultSelector)
    {
        ArgumentNullException.ThrowIfNull(collectionSelector);
        ArgumentNullException.ThrowIfNull(resultSelector);

        return Bind(success => collectionSelector(success).Map(value => resultSelector(success, value)));
    }

    /// <summary>
    /// Invokes <paramref name="onSuccess"/> for a success value and returns this result unchanged.
    /// </summary>
    /// <param name="onSuccess">The side effect to run for a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TValue, TError> Tap(Action<TValue> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);

        switch (Value)
        {
            case TValue success:
                onSuccess(success);
                return this;
            case TError:
                return this;
            default:
                throw new InvalidOperationException("The result does not hold a value.");
        }
    }

    /// <summary>
    /// Invokes <paramref name="onSuccess"/> or <paramref name="onError"/> depending on the case
    /// and returns this result unchanged.
    /// </summary>
    /// <param name="onSuccess">The side effect to run for a success value.</param>
    /// <param name="onError">The side effect to run for an error value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TValue, TError> Tap(Action<TValue> onSuccess, Action<TError> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        switch (Value)
        {
            case TValue success:
                onSuccess(success);
                return this;
            case TError failure:
                onError(failure);
                return this;
            default:
                throw new InvalidOperationException("The result does not hold a value.");
        }
    }

    /// <summary>
    /// Invokes <paramref name="onError"/> for an error value and returns this result unchanged.
    /// </summary>
    /// <param name="onError">The side effect to run for an error value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TValue, TError> TapError(Action<TError> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);

        switch (Value)
        {
            case TError failure:
                onError(failure);
                return this;
            case TValue:
                return this;
            default:
                throw new InvalidOperationException("The result does not hold a value.");
        }
    }

    /// <summary>
    /// Returns this result when it is successful and <paramref name="predicate"/> returns
    /// <see langword="true"/>; otherwise, replaces the success with <paramref name="error"/>.
    /// Error values are propagated unchanged.
    /// </summary>
    /// <param name="predicate">The condition the success value must satisfy.</param>
    /// <param name="error">The error to return when the condition fails.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TValue, TError> Ensure(Func<TValue, bool> predicate, TError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return Value switch
        {
            TValue success when predicate(success) => this,
            TValue => new Result<TValue, TError>(error),
            TError => this,
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Returns this result when it is successful and <paramref name="predicate"/> returns
    /// <see langword="true"/>; otherwise, creates an error with <paramref name="errorFactory"/>.
    /// Error values are propagated unchanged.
    /// </summary>
    /// <param name="predicate">The condition the success value must satisfy.</param>
    /// <param name="errorFactory">Builds the error from the success value when the condition fails.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public Result<TValue, TError> Ensure(Func<TValue, bool> predicate, Func<TValue, TError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);

        return Value switch
        {
            TValue success when predicate(success) => this,
            TValue success => new Result<TValue, TError>(errorFactory(success)),
            TError => this,
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <inheritdoc />
    public override string ToString() =>
        Value switch
        {
            TValue success => $"Success({success})",
            TError failure => $"Error({failure})",
            _ => "Empty"
        };

    /// <inheritdoc />
    public bool Equals(Result<TValue, TError> other) =>
        EqualityComparer<object?>.Default.Equals(Value, other.Value);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Result<TValue, TError> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    /// <summary>Returns <see langword="true"/> when both results hold equal values.</summary>
    public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right) => left.Equals(right);

    /// <summary>Returns <see langword="true"/> when both results hold non-equal values.</summary>
    public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right) => !left.Equals(right);
}
