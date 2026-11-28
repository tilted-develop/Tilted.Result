namespace Tilted.Result;

/// <summary>
/// Asynchronous factory methods for <see cref="Result{TValue, TError}"/>.
/// </summary>
public static partial class Result
{
    /// <summary>
    /// Asynchronously executes <paramref name="factory"/> and captures a thrown exception
    /// as an error of type <see cref="Exception"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="factory">The asynchronous function that produces the success value.</param>
    /// <returns>A successful result, or a failed result that contains the thrown exception.</returns>
    public static async ValueTask<Result<TValue, Exception>> TryAsync<TValue>(Func<ValueTask<TValue>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        TValue value;
        try
        {
            value = await factory().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            return new Result<TValue, Exception>(exception);
        }

        return new Result<TValue, Exception>(value);
    }

    /// <summary>
    /// Asynchronously executes <paramref name="factory"/> and maps a thrown exception
    /// to an error using <paramref name="onError"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <typeparam name="TError">The user-defined type of the error value.</typeparam>
    /// <param name="factory">The asynchronous function that produces the success value.</param>
    /// <param name="onError">Maps a caught exception to an error value.</param>
    /// <returns>A successful result, or a failed result produced from the thrown exception.</returns>
    public static async ValueTask<Result<TValue, TError>> TryAsync<TValue, TError>(
        Func<ValueTask<TValue>> factory,
        Func<Exception, TError> onError)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(onError);

        TValue value;
        try
        {
            value = await factory().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            return new Result<TValue, TError>(onError(exception));
        }

        return new Result<TValue, TError>(value);
    }
}
