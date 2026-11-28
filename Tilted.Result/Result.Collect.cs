namespace Tilted.Result;

/// <summary>
/// Helpers for combining multiple <see cref="Result{TValue, TError}"/> values.
/// </summary>
public static partial class Result
{
    /// <summary>
    /// Combines <paramref name="results"/> into a single success list, returning the first error.
    /// </summary>
    /// <exception cref="InvalidOperationException">A result in the sequence is a <c>default</c> value.</exception>
    public static Result<IReadOnlyList<TValue>, TError> Collect<TValue, TError>(
        IEnumerable<Result<TValue, TError>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = new List<TValue>();
        foreach (var result in results)
        {
            if (!result.HasValue)
            {
                throw new InvalidOperationException("The result does not hold a value.");
            }

            if (result.TryGetError(out var error))
            {
                return new Result<IReadOnlyList<TValue>, TError>(error);
            }

            values.Add(result.Unwrap());
        }

        return new Result<IReadOnlyList<TValue>, TError>(values);
    }

    /// <summary>Combines <paramref name="results"/> into a single success list, returning the first error.</summary>
    public static Result<IReadOnlyList<TValue>, TError> Collect<TValue, TError>(
        params Result<TValue, TError>[] results) =>
        Collect((IEnumerable<Result<TValue, TError>>)results);

    /// <summary>
    /// Combines <paramref name="results"/> into a single success list, accumulating every error.
    /// </summary>
    /// <exception cref="InvalidOperationException">A result in the sequence is a <c>default</c> value.</exception>
    public static Result<IReadOnlyList<TValue>, IReadOnlyList<TError>> CollectAll<TValue, TError>(
        IEnumerable<Result<TValue, TError>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var values = new List<TValue>();
        var errors = new List<TError>();
        foreach (var result in results)
        {
            if (!result.HasValue)
            {
                throw new InvalidOperationException("The result does not hold a value.");
            }

            if (result.TryGetError(out var error))
            {
                errors.Add(error);
            }
            else
            {
                values.Add(result.Unwrap());
            }
        }

        return errors.Count > 0
            ? new Result<IReadOnlyList<TValue>, IReadOnlyList<TError>>(errors)
            : new Result<IReadOnlyList<TValue>, IReadOnlyList<TError>>(values);
    }

    /// <summary>Combines <paramref name="results"/> into a single success list, accumulating every error.</summary>
    public static Result<IReadOnlyList<TValue>, IReadOnlyList<TError>> CollectAll<TValue, TError>(
        params Result<TValue, TError>[] results) =>
        CollectAll((IEnumerable<Result<TValue, TError>>)results);

    /// <summary>Combines results into a named tuple, returning the first error.</summary>
    public static Result<(T1 First, T2 Second), TError> Combine<T1, T2, TError>(
        Result<T1, TError> result1,
        Result<T2, TError> result2)
    {
        if (!result1.HasValue || !result2.HasValue)
        {
            throw new InvalidOperationException("The result does not hold a value.");
        }

        if (result1.TryGetError(out var error1))
        {
            return new Result<(T1 First, T2 Second), TError>(error1);
        }
        if (result2.TryGetError(out var error2))
        {
            return new Result<(T1 First, T2 Second), TError>(error2);
        }

        return new Result<(T1 First, T2 Second), TError>((result1.Unwrap(), result2.Unwrap()));
    }

    /// <summary>Combines results into a named tuple, returning the first error.</summary>
    public static Result<(T1 First, T2 Second, T3 Third), TError> Combine<T1, T2, T3, TError>(
        Result<T1, TError> result1,
        Result<T2, TError> result2,
        Result<T3, TError> result3)
    {
        if (!result1.HasValue || !result2.HasValue || !result3.HasValue)
        {
            throw new InvalidOperationException("The result does not hold a value.");
        }

        if (result1.TryGetError(out var error1))
        {
            return new Result<(T1 First, T2 Second, T3 Third), TError>(error1);
        }
        if (result2.TryGetError(out var error2))
        {
            return new Result<(T1 First, T2 Second, T3 Third), TError>(error2);
        }
        if (result3.TryGetError(out var error3))
        {
            return new Result<(T1 First, T2 Second, T3 Third), TError>(error3);
        }

        return new Result<(T1 First, T2 Second, T3 Third), TError>((result1.Unwrap(), result2.Unwrap(), result3.Unwrap()));
    }

    /// <summary>Combines results into a named tuple, returning the first error.</summary>
    public static Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError> Combine<T1, T2, T3, T4, TError>(
        Result<T1, TError> result1,
        Result<T2, TError> result2,
        Result<T3, TError> result3,
        Result<T4, TError> result4)
    {
        if (!result1.HasValue || !result2.HasValue || !result3.HasValue || !result4.HasValue)
        {
            throw new InvalidOperationException("The result does not hold a value.");
        }

        if (result1.TryGetError(out var error1))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError>(error1);
        }
        if (result2.TryGetError(out var error2))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError>(error2);
        }
        if (result3.TryGetError(out var error3))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError>(error3);
        }
        if (result4.TryGetError(out var error4))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError>(error4);
        }

        return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError>((result1.Unwrap(), result2.Unwrap(), result3.Unwrap(), result4.Unwrap()));
    }

    /// <summary>Combines results into a named tuple, returning the first error.</summary>
    public static Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth), TError> Combine<T1, T2, T3, T4, T5, TError>(
        Result<T1, TError> result1,
        Result<T2, TError> result2,
        Result<T3, TError> result3,
        Result<T4, TError> result4,
        Result<T5, TError> result5)
    {
        if (!result1.HasValue || !result2.HasValue || !result3.HasValue || !result4.HasValue || !result5.HasValue)
        {
            throw new InvalidOperationException("The result does not hold a value.");
        }

        if (result1.TryGetError(out var error1))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth), TError>(error1);
        }
        if (result2.TryGetError(out var error2))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth), TError>(error2);
        }
        if (result3.TryGetError(out var error3))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth), TError>(error3);
        }
        if (result4.TryGetError(out var error4))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth), TError>(error4);
        }
        if (result5.TryGetError(out var error5))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth), TError>(error5);
        }

        return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth), TError>((result1.Unwrap(), result2.Unwrap(), result3.Unwrap(), result4.Unwrap(), result5.Unwrap()));
    }

    /// <summary>Combines results into a named tuple, returning the first error.</summary>
    public static Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError> Combine<T1, T2, T3, T4, T5, T6, TError>(
        Result<T1, TError> result1,
        Result<T2, TError> result2,
        Result<T3, TError> result3,
        Result<T4, TError> result4,
        Result<T5, TError> result5,
        Result<T6, TError> result6)
    {
        if (!result1.HasValue || !result2.HasValue || !result3.HasValue || !result4.HasValue || !result5.HasValue || !result6.HasValue)
        {
            throw new InvalidOperationException("The result does not hold a value.");
        }

        if (result1.TryGetError(out var error1))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError>(error1);
        }
        if (result2.TryGetError(out var error2))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError>(error2);
        }
        if (result3.TryGetError(out var error3))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError>(error3);
        }
        if (result4.TryGetError(out var error4))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError>(error4);
        }
        if (result5.TryGetError(out var error5))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError>(error5);
        }
        if (result6.TryGetError(out var error6))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError>(error6);
        }

        return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth), TError>((result1.Unwrap(), result2.Unwrap(), result3.Unwrap(), result4.Unwrap(), result5.Unwrap(), result6.Unwrap()));
    }

    /// <summary>Combines results into a named tuple, returning the first error.</summary>
    public static Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError> Combine<T1, T2, T3, T4, T5, T6, T7, TError>(
        Result<T1, TError> result1,
        Result<T2, TError> result2,
        Result<T3, TError> result3,
        Result<T4, TError> result4,
        Result<T5, TError> result5,
        Result<T6, TError> result6,
        Result<T7, TError> result7)
    {
        if (!result1.HasValue || !result2.HasValue || !result3.HasValue || !result4.HasValue || !result5.HasValue || !result6.HasValue || !result7.HasValue)
        {
            throw new InvalidOperationException("The result does not hold a value.");
        }

        if (result1.TryGetError(out var error1))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>(error1);
        }
        if (result2.TryGetError(out var error2))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>(error2);
        }
        if (result3.TryGetError(out var error3))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>(error3);
        }
        if (result4.TryGetError(out var error4))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>(error4);
        }
        if (result5.TryGetError(out var error5))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>(error5);
        }
        if (result6.TryGetError(out var error6))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>(error6);
        }
        if (result7.TryGetError(out var error7))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>(error7);
        }

        return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh), TError>((result1.Unwrap(), result2.Unwrap(), result3.Unwrap(), result4.Unwrap(), result5.Unwrap(), result6.Unwrap(), result7.Unwrap()));
    }

    /// <summary>Combines results into a named tuple, returning the first error.</summary>
    public static Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError> Combine<T1, T2, T3, T4, T5, T6, T7, T8, TError>(
        Result<T1, TError> result1,
        Result<T2, TError> result2,
        Result<T3, TError> result3,
        Result<T4, TError> result4,
        Result<T5, TError> result5,
        Result<T6, TError> result6,
        Result<T7, TError> result7,
        Result<T8, TError> result8)
    {
        if (!result1.HasValue || !result2.HasValue || !result3.HasValue || !result4.HasValue || !result5.HasValue || !result6.HasValue || !result7.HasValue || !result8.HasValue)
        {
            throw new InvalidOperationException("The result does not hold a value.");
        }

        if (result1.TryGetError(out var error1))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error1);
        }
        if (result2.TryGetError(out var error2))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error2);
        }
        if (result3.TryGetError(out var error3))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error3);
        }
        if (result4.TryGetError(out var error4))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error4);
        }
        if (result5.TryGetError(out var error5))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error5);
        }
        if (result6.TryGetError(out var error6))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error6);
        }
        if (result7.TryGetError(out var error7))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error7);
        }
        if (result8.TryGetError(out var error8))
        {
            return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>(error8);
        }

        return new Result<(T1 First, T2 Second, T3 Third, T4 Fourth, T5 Fifth, T6 Sixth, T7 Seventh, T8 Eighth), TError>((result1.Unwrap(), result2.Unwrap(), result3.Unwrap(), result4.Unwrap(), result5.Unwrap(), result6.Unwrap(), result7.Unwrap(), result8.Unwrap()));
    }

    /// <summary>Combines a sequence of asynchronous results into a single success list, returning the first error.</summary>
    public static async ValueTask<Result<IReadOnlyList<TValue>, TError>> Collect<TValue, TError>(
        IEnumerable<ValueTask<Result<TValue, TError>>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var tasks = results.Select(result => result.AsTask()).ToList();

        var completed = await Task.WhenAll(tasks).ConfigureAwait(false);
        return Collect((IEnumerable<Result<TValue, TError>>)completed);
    }

    /// <summary>Combines a sequence of asynchronous results into a single success list, returning the first error.</summary>
    public static async ValueTask<Result<IReadOnlyList<TValue>, TError>> Collect<TValue, TError>(
        IEnumerable<Task<Result<TValue, TError>>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var completed = await Task.WhenAll(results).ConfigureAwait(false);
        return Collect((IEnumerable<Result<TValue, TError>>)completed);
    }

    /// <summary>Combines a sequence of asynchronous results, accumulating every error.</summary>
    public static async ValueTask<Result<IReadOnlyList<TValue>, IReadOnlyList<TError>>> CollectAll<TValue, TError>(
        IEnumerable<ValueTask<Result<TValue, TError>>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var tasks = results.Select(result => result.AsTask()).ToList();

        var completed = await Task.WhenAll(tasks).ConfigureAwait(false);
        return CollectAll((IEnumerable<Result<TValue, TError>>)completed);
    }

    /// <summary>Combines a sequence of asynchronous results, accumulating every error.</summary>
    public static async ValueTask<Result<IReadOnlyList<TValue>, IReadOnlyList<TError>>> CollectAll<TValue, TError>(
        IEnumerable<Task<Result<TValue, TError>>> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var completed = await Task.WhenAll(results).ConfigureAwait(false);
        return CollectAll((IEnumerable<Result<TValue, TError>>)completed);
    }

    /// <summary>Asynchronously combines results into a named tuple, returning the first error.</summary>
    public static async ValueTask<Result<(T1 First, T2 Second), TError>> Combine<T1, T2, TError>(
        ValueTask<Result<T1, TError>> result1,
        ValueTask<Result<T2, TError>> result2)
    {
        var task1 = result1.AsTask();
        var task2 = result2.AsTask();

        await Task.WhenAll(task1, task2).ConfigureAwait(false);
        return Combine(await task1.ConfigureAwait(false), await task2.ConfigureAwait(false));
    }

    /// <summary>Asynchronously combines results into a named tuple, returning the first error.</summary>
    public static async ValueTask<Result<(T1 First, T2 Second), TError>> Combine<T1, T2, TError>(
        Task<Result<T1, TError>> result1,
        Task<Result<T2, TError>> result2)
    {
        var task1 = result1;
        var task2 = result2;

        await Task.WhenAll(task1, task2).ConfigureAwait(false);
        return Combine(await task1.ConfigureAwait(false), await task2.ConfigureAwait(false));
    }

    /// <summary>Asynchronously combines results into a named tuple, returning the first error.</summary>
    public static async ValueTask<Result<(T1 First, T2 Second, T3 Third), TError>> Combine<T1, T2, T3, TError>(
        ValueTask<Result<T1, TError>> result1,
        ValueTask<Result<T2, TError>> result2,
        ValueTask<Result<T3, TError>> result3)
    {
        var task1 = result1.AsTask();
        var task2 = result2.AsTask();
        var task3 = result3.AsTask();

        await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
        return Combine(await task1.ConfigureAwait(false), await task2.ConfigureAwait(false), await task3.ConfigureAwait(false));
    }

    /// <summary>Asynchronously combines results into a named tuple, returning the first error.</summary>
    public static async ValueTask<Result<(T1 First, T2 Second, T3 Third), TError>> Combine<T1, T2, T3, TError>(
        Task<Result<T1, TError>> result1,
        Task<Result<T2, TError>> result2,
        Task<Result<T3, TError>> result3)
    {
        var task1 = result1;
        var task2 = result2;
        var task3 = result3;

        await Task.WhenAll(task1, task2, task3).ConfigureAwait(false);
        return Combine(await task1.ConfigureAwait(false), await task2.ConfigureAwait(false), await task3.ConfigureAwait(false));
    }

    /// <summary>Asynchronously combines results into a named tuple, returning the first error.</summary>
    public static async ValueTask<Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError>> Combine<T1, T2, T3, T4, TError>(
        ValueTask<Result<T1, TError>> result1,
        ValueTask<Result<T2, TError>> result2,
        ValueTask<Result<T3, TError>> result3,
        ValueTask<Result<T4, TError>> result4)
    {
        var task1 = result1.AsTask();
        var task2 = result2.AsTask();
        var task3 = result3.AsTask();
        var task4 = result4.AsTask();

        await Task.WhenAll(task1, task2, task3, task4).ConfigureAwait(false);
        return Combine(await task1.ConfigureAwait(false), await task2.ConfigureAwait(false), await task3.ConfigureAwait(false), await task4.ConfigureAwait(false));
    }

    /// <summary>Asynchronously combines results into a named tuple, returning the first error.</summary>
    public static async ValueTask<Result<(T1 First, T2 Second, T3 Third, T4 Fourth), TError>> Combine<T1, T2, T3, T4, TError>(
        Task<Result<T1, TError>> result1,
        Task<Result<T2, TError>> result2,
        Task<Result<T3, TError>> result3,
        Task<Result<T4, TError>> result4)
    {
        var task1 = result1;
        var task2 = result2;
        var task3 = result3;
        var task4 = result4;

        await Task.WhenAll(task1, task2, task3, task4).ConfigureAwait(false);
        return Combine(await task1.ConfigureAwait(false), await task2.ConfigureAwait(false), await task3.ConfigureAwait(false), await task4.ConfigureAwait(false));
    }

}
