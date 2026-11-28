namespace Tilted.Result;

/// <summary>
/// Composition helpers that let synchronous and asynchronous <see cref="Result{TValue, TError}"/>
/// values be chained together. Base member names accept synchronous continuations; members with
/// the <c>Async</c> suffix accept asynchronous continuations. Every member works on both
/// <see cref="ValueTask{TResult}"/> and <see cref="Task{TResult}"/> sources.
/// </summary>
public static class ResultAsyncExtensions
{
    /// <summary>Maps the success value; errors are propagated unchanged.</summary>
    public static async ValueTask<Result<TResult, TError>> Map<TValue, TError, TResult>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var result = await source.ConfigureAwait(false);
        return result.Map(selector);
    }

    /// <summary>Maps the success value; errors are propagated unchanged. (Task source).</summary>
    public static ValueTask<Result<TResult, TError>> Map<TValue, TError, TResult>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, TResult> selector)
        => new ValueTask<Result<TValue, TError>>(source).Map(selector);

    /// <summary>Maps the success value; errors are propagated unchanged. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TResult, TError>> MapAsync<TValue, TError, TResult>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, ValueTask<TResult>> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        var result = await source.ConfigureAwait(false);
        return await result.MapAsync(selector).ConfigureAwait(false);
    }

    /// <summary>Maps the success value; errors are propagated unchanged. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TResult, TError>> MapAsync<TValue, TError, TResult>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, ValueTask<TResult>> selector)
        => new ValueTask<Result<TValue, TError>>(source).MapAsync(selector);

    /// <summary>Binds the success value; errors are propagated unchanged.</summary>
    public static async ValueTask<Result<TResult, TError>> Bind<TValue, TError, TResult>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, Result<TResult, TError>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);

        var result = await source.ConfigureAwait(false);
        return result.Bind(bind);
    }

    /// <summary>Binds the success value; errors are propagated unchanged. (Task source).</summary>
    public static ValueTask<Result<TResult, TError>> Bind<TValue, TError, TResult>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, Result<TResult, TError>> bind)
        => new ValueTask<Result<TValue, TError>>(source).Bind(bind);

    /// <summary>Binds the success value; errors are propagated unchanged. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TResult, TError>> BindAsync<TValue, TError, TResult>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, ValueTask<Result<TResult, TError>>> bind)
    {
        ArgumentNullException.ThrowIfNull(bind);

        var result = await source.ConfigureAwait(false);
        return await result.BindAsync(bind).ConfigureAwait(false);
    }

    /// <summary>Binds the success value; errors are propagated unchanged. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TResult, TError>> BindAsync<TValue, TError, TResult>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, ValueTask<Result<TResult, TError>>> bind)
        => new ValueTask<Result<TValue, TError>>(source).BindAsync(bind);

    /// <summary>Maps the error value; success values are propagated unchanged.</summary>
    public static async ValueTask<Result<TValue, TNewError>> MapError<TValue, TError, TNewError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TError, TNewError> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var result = await source.ConfigureAwait(false);
        return result.MapError(map);
    }

    /// <summary>Maps the error value; success values are propagated unchanged. (Task source).</summary>
    public static ValueTask<Result<TValue, TNewError>> MapError<TValue, TError, TNewError>(
        this Task<Result<TValue, TError>> source,
        Func<TError, TNewError> map)
        => new ValueTask<Result<TValue, TError>>(source).MapError(map);

    /// <summary>Maps the error value; success values are propagated unchanged. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TValue, TNewError>> MapErrorAsync<TValue, TError, TNewError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TError, ValueTask<TNewError>> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        var result = await source.ConfigureAwait(false);
        return await result.MapErrorAsync(map).ConfigureAwait(false);
    }

    /// <summary>Maps the error value; success values are propagated unchanged. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TValue, TNewError>> MapErrorAsync<TValue, TError, TNewError>(
        this Task<Result<TValue, TError>> source,
        Func<TError, ValueTask<TNewError>> map)
        => new ValueTask<Result<TValue, TError>>(source).MapErrorAsync(map);

    /// <summary>Projects the result into a single value.</summary>
    public static async ValueTask<TResult> Match<TValue, TError, TResult>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, TResult> onSuccess, Func<TError, TResult> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        var result = await source.ConfigureAwait(false);
        return result.Match(onSuccess, onError);
    }

    /// <summary>Projects the result into a single value. (Task source).</summary>
    public static ValueTask<TResult> Match<TValue, TError, TResult>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, TResult> onSuccess, Func<TError, TResult> onError)
        => new ValueTask<Result<TValue, TError>>(source).Match(onSuccess, onError);

    /// <summary>Projects the result into a single value. (asynchronous continuation).</summary>
    public static async ValueTask<TResult> MatchAsync<TValue, TError, TResult>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, ValueTask<TResult>> onSuccess, Func<TError, ValueTask<TResult>> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        var result = await source.ConfigureAwait(false);
        return await result.MatchAsync(onSuccess, onError).ConfigureAwait(false);
    }

    /// <summary>Projects the result into a single value. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<TResult> MatchAsync<TValue, TError, TResult>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, ValueTask<TResult>> onSuccess, Func<TError, ValueTask<TResult>> onError)
        => new ValueTask<Result<TValue, TError>>(source).MatchAsync(onSuccess, onError);

    /// <summary>Runs a side effect and returns the same result.</summary>
    public static async ValueTask<Result<TValue, TError>> Tap<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Action<TValue> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);

        var result = await source.ConfigureAwait(false);
        return result.Tap(onSuccess);
    }

    /// <summary>Runs a side effect and returns the same result. (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> Tap<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Action<TValue> onSuccess)
        => new ValueTask<Result<TValue, TError>>(source).Tap(onSuccess);

    /// <summary>Runs a side effect and returns the same result.</summary>
    public static async ValueTask<Result<TValue, TError>> Tap<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Action<TValue> onSuccess, Action<TError> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        var result = await source.ConfigureAwait(false);
        return result.Tap(onSuccess, onError);
    }

    /// <summary>Runs a side effect and returns the same result. (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> Tap<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Action<TValue> onSuccess, Action<TError> onError)
        => new ValueTask<Result<TValue, TError>>(source).Tap(onSuccess, onError);

    /// <summary>Runs a side effect and returns the same result. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TValue, TError>> TapAsync<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, ValueTask> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);

        var result = await source.ConfigureAwait(false);
        return await result.TapAsync(onSuccess).ConfigureAwait(false);
    }

    /// <summary>Runs a side effect and returns the same result. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> TapAsync<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, ValueTask> onSuccess)
        => new ValueTask<Result<TValue, TError>>(source).TapAsync(onSuccess);

    /// <summary>Runs a side effect and returns the same result. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TValue, TError>> TapAsync<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, ValueTask> onSuccess, Func<TError, ValueTask> onError)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onError);

        var result = await source.ConfigureAwait(false);
        return await result.TapAsync(onSuccess, onError).ConfigureAwait(false);
    }

    /// <summary>Runs a side effect and returns the same result. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> TapAsync<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, ValueTask> onSuccess, Func<TError, ValueTask> onError)
        => new ValueTask<Result<TValue, TError>>(source).TapAsync(onSuccess, onError);

    /// <summary>Runs a side effect on an error value and returns the same result.</summary>
    public static async ValueTask<Result<TValue, TError>> TapError<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Action<TError> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);

        var result = await source.ConfigureAwait(false);
        return result.TapError(onError);
    }

    /// <summary>Runs a side effect on an error value and returns the same result. (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> TapError<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Action<TError> onError)
        => new ValueTask<Result<TValue, TError>>(source).TapError(onError);

    /// <summary>Runs a side effect on an error value and returns the same result. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TValue, TError>> TapErrorAsync<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TError, ValueTask> onError)
    {
        ArgumentNullException.ThrowIfNull(onError);

        var result = await source.ConfigureAwait(false);
        return await result.TapErrorAsync(onError).ConfigureAwait(false);
    }

    /// <summary>Runs a side effect on an error value and returns the same result. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> TapErrorAsync<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TError, ValueTask> onError)
        => new ValueTask<Result<TValue, TError>>(source).TapErrorAsync(onError);

    /// <summary>Fails a success that does not satisfy the predicate.</summary>
    public static async ValueTask<Result<TValue, TError>> Ensure<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, bool> predicate, TError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var result = await source.ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Fails a success that does not satisfy the predicate. (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> Ensure<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, bool> predicate, TError error)
        => new ValueTask<Result<TValue, TError>>(source).Ensure(predicate, error);

    /// <summary>Fails a success that does not satisfy the predicate.</summary>
    public static async ValueTask<Result<TValue, TError>> Ensure<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, bool> predicate, Func<TValue, TError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);

        var result = await source.ConfigureAwait(false);
        return result.Ensure(predicate, errorFactory);
    }

    /// <summary>Fails a success that does not satisfy the predicate. (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> Ensure<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, bool> predicate, Func<TValue, TError> errorFactory)
        => new ValueTask<Result<TValue, TError>>(source).Ensure(predicate, errorFactory);

    /// <summary>Fails a success that does not satisfy the predicate. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TValue, TError>> EnsureAsync<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, ValueTask<bool>> predicate, TError error)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        var result = await source.ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error).ConfigureAwait(false);
    }

    /// <summary>Fails a success that does not satisfy the predicate. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> EnsureAsync<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, ValueTask<bool>> predicate, TError error)
        => new ValueTask<Result<TValue, TError>>(source).EnsureAsync(predicate, error);

    /// <summary>Fails a success that does not satisfy the predicate. (asynchronous continuation).</summary>
    public static async ValueTask<Result<TValue, TError>> EnsureAsync<TValue, TError>(
        this ValueTask<Result<TValue, TError>> source,
        Func<TValue, ValueTask<bool>> predicate, Func<TValue, TError> errorFactory)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(errorFactory);

        var result = await source.ConfigureAwait(false);
        return await result.EnsureAsync(predicate, errorFactory).ConfigureAwait(false);
    }

    /// <summary>Fails a success that does not satisfy the predicate. (asynchronous continuation). (Task source).</summary>
    public static ValueTask<Result<TValue, TError>> EnsureAsync<TValue, TError>(
        this Task<Result<TValue, TError>> source,
        Func<TValue, ValueTask<bool>> predicate, Func<TValue, TError> errorFactory)
        => new ValueTask<Result<TValue, TError>>(source).EnsureAsync(predicate, errorFactory);

}
