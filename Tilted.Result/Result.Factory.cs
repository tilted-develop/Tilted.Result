namespace Tilted.Result;

/// <summary>
/// Provides factory methods for creating <see cref="Result{TValue, TError}"/> values.
/// </summary>
public static partial class Result
{
    /// <summary>Creates a successful result that contains <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <typeparam name="TError">The user-defined type of the error value.</typeparam>
    /// <param name="value">The success value. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static Result<TValue, TError> Ok<TValue, TError>(TValue value) => new(value);

    /// <summary>Creates a failed result that contains <paramref name="error"/>.</summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <typeparam name="TError">The user-defined type of the error value.</typeparam>
    /// <param name="error">The error value. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<TValue, TError> Fail<TValue, TError>(TError error) => new(error);

    /// <summary>Creates a failed result from exception <paramref name="exception"/>.</summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="exception">The exception that describes the failure.</param>
    public static Result<TValue, Exception> Fail<TValue>(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new Result<TValue, Exception>(exception);
    }

    /// <summary>
    /// Executes <paramref name="factory"/> and captures a thrown exception as an error of type
    /// <see cref="Exception"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="factory">The function that produces the success value.</param>
    /// <returns>A successful result, or a failed result that contains the thrown exception.</returns>
    public static Result<TValue, Exception> Try<TValue>(Func<TValue> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        TValue value;
        try
        {
            value = factory();
        }
        catch (Exception exception)
        {
            return new Result<TValue, Exception>(exception);
        }

        return new Result<TValue, Exception>(value);
    }

    /// <summary>
    /// Executes <paramref name="factory"/> and maps a thrown exception to an error using
    /// <paramref name="onError"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <typeparam name="TError">The user-defined type of the error value.</typeparam>
    /// <param name="factory">The function that produces the success value.</param>
    /// <param name="onError">Maps a caught exception to an error value.</param>
    /// <returns>A successful result, or a failed result produced from the thrown exception.</returns>
    public static Result<TValue, TError> Try<TValue, TError>(Func<TValue> factory, Func<Exception, TError> onError)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(onError);

        TValue value;
        try
        {
            value = factory();
        }
        catch (Exception exception)
        {
            return new Result<TValue, TError>(onError(exception));
        }

        return new Result<TValue, TError>(value);
    }
}
