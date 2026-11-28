namespace Tilted.Result;

/// <summary>
/// Asynchronous members of <see cref="Result{TValue, TError}"/>.
/// </summary>
public readonly partial struct Result<TValue, TError>
{
    /// <summary>
    /// Asynchronously maps the success value using <paramref name="map"/>.
    /// Error values are propagated unchanged.
    /// </summary>
    /// <typeparam name="TResult">The type of the mapped success value.</typeparam>
    /// <param name="map">The asynchronous transformation to apply to a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TResult, TError>> MapAsync<TResult>(Func<TValue, ValueTask<TResult>> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return Value switch
        {
            TValue success => new Result<TResult, TError>(await map(success).ConfigureAwait(false)),
            TError failure => new Result<TResult, TError>(failure),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Asynchronously binds the success value using <paramref name="bind"/>.
    /// Error values are propagated unchanged.
    /// </summary>
    /// <typeparam name="TResult">The type of the bound success value.</typeparam>
    /// <param name="bind">The asynchronous continuation to apply to a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TResult, TError>> BindAsync<TResult>(
        Func<TValue, ValueTask<Result<TResult, TError>>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);

        return Value switch
        {
            TValue success => await bind(success).ConfigureAwait(false),
            TError failure => new Result<TResult, TError>(failure),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Asynchronously maps the error value using <paramref name="map"/>.
    /// Success values are propagated unchanged.
    /// </summary>
    /// <typeparam name="TNewError">The type of the mapped error value.</typeparam>
    /// <param name="map">The asynchronous transformation to apply to an error value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TValue, TNewError>> MapErrorAsync<TNewError>(Func<TError, ValueTask<TNewError>> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return Value switch
        {
            TValue success => new Result<TValue, TNewError>(success),
            TError failure => new Result<TValue, TNewError>(await map(failure).ConfigureAwait(false)),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Asynchronously projects this result into a single value by applying one of the supplied functions.
    /// </summary>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="onSuccess">The asynchronous function to apply to a success value.</param>
    /// <param name="onError">The asynchronous function to apply to an error value.</param>
    /// <exception cref="InvalidOperationException">This result does not hold any value.</exception>
    public async ValueTask<TResult> MatchAsync<TResult>(
        Func<TValue, ValueTask<TResult>> onSuccess,
        Func<TError, ValueTask<TResult>> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        return Value switch
        {
            TValue success => await onSuccess(success).ConfigureAwait(false),
            TError failure => await onError(failure).ConfigureAwait(false),
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Asynchronously invokes <paramref name="onSuccess"/> for a success value and returns this result unchanged.
    /// </summary>
    /// <param name="onSuccess">The asynchronous side effect to run for a success value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TValue, TError>> TapAsync(Func<TValue, ValueTask> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);

        switch (Value)
        {
            case TValue success:
                await onSuccess(success).ConfigureAwait(false);
                return this;
            case TError:
                return this;
            default:
                throw new InvalidOperationException("The result does not hold a value.");
        }
    }

    /// <summary>
    /// Asynchronously invokes <paramref name="onSuccess"/> or <paramref name="onError"/> depending on
    /// the case and returns this result unchanged.
    /// </summary>
    /// <param name="onSuccess">The asynchronous side effect to run for a success value.</param>
    /// <param name="onError">The asynchronous side effect to run for an error value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TValue, TError>> TapAsync(
        Func<TValue, ValueTask> onSuccess,
        Func<TError, ValueTask> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        switch (Value)
        {
            case TValue success:
                await onSuccess(success).ConfigureAwait(false);
                return this;
            case TError failure:
                await onError(failure).ConfigureAwait(false);
                return this;
            default:
                throw new InvalidOperationException("The result does not hold a value.");
        }
    }

    /// <summary>
    /// Asynchronously invokes <paramref name="onError"/> for an error value and returns this result unchanged.
    /// </summary>
    /// <param name="onError">The asynchronous side effect to run for an error value.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TValue, TError>> TapErrorAsync(Func<TError, ValueTask> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);

        switch (Value)
        {
            case TError failure:
                await onError(failure).ConfigureAwait(false);
                return this;
            case TValue:
                return this;
            default:
                throw new InvalidOperationException("The result does not hold a value.");
        }
    }

    /// <summary>
    /// Asynchronously returns this result when it is successful and <paramref name="predicate"/> returns
    /// <see langword="true"/>; otherwise, replaces the success with <paramref name="error"/>.
    /// Error values are propagated unchanged.
    /// </summary>
    /// <param name="predicate">The asynchronous condition the success value must satisfy.</param>
    /// <param name="error">The error to return when the condition fails.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TValue, TError>> EnsureAsync(Func<TValue, ValueTask<bool>> predicate, TError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return Value switch
        {
            TValue success => await predicate(success).ConfigureAwait(false) ? this : new Result<TValue, TError>(error),
            TError => this,
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }

    /// <summary>
    /// Asynchronously returns this result when it is successful and <paramref name="predicate"/> returns
    /// <see langword="true"/>; otherwise, creates an error with <paramref name="errorFactory"/>.
    /// Error values are propagated unchanged.
    /// </summary>
    /// <param name="predicate">The asynchronous condition the success value must satisfy.</param>
    /// <param name="errorFactory">Builds the error from the success value when the condition fails.</param>
    /// <exception cref="InvalidOperationException">This result is a <c>default</c> value without any value.</exception>
    public async ValueTask<Result<TValue, TError>> EnsureAsync(
        Func<TValue, ValueTask<bool>> predicate,
        Func<TValue, TError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);

        return Value switch
        {
            TValue success => await predicate(success).ConfigureAwait(false)
                ? this
                : new Result<TValue, TError>(errorFactory(success)),
            TError => this,
            _ => throw new InvalidOperationException("The result does not hold a value.")
        };
    }
}
