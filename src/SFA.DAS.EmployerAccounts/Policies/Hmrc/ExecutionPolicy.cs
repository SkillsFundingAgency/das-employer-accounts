using Polly;

namespace SFA.DAS.EmployerAccounts.Policies.Hmrc;

public abstract class ExecutionPolicy
{
    protected IAsyncPolicy RootPolicy { get; set; }
    
    public virtual async Task ExecuteAsync(Func<Task> action)
    {
        try
        {
            await RootPolicy.ExecuteAsync(action);
        }
        catch (Exception ex)
        {
            OnException(ex);
        }
    }

    public virtual async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
    {
        try
        {
            return await RootPolicy.ExecuteAsync(func);
        }
        catch (Exception ex)
        {
            return OnException<T>(ex);
        }
    }


    protected virtual void OnException(Exception ex)
    {
        throw ex;
    }

    protected virtual T OnException<T>(Exception ex)
    {
        throw ex;
    }

    protected static IAsyncPolicy CreateAsyncRetryPolicy<T>(Func<T, bool> canHandle, int numberOfRetries, TimeSpan waitBetweenTries, Action<Exception> onRetryableFailure = null)
        where T : Exception
    {
        var waits = new TimeSpan[numberOfRetries];
        for (var retryCount = 0; retryCount < waits.Length; retryCount++)
        {
            waits[retryCount] = waitBetweenTries;
        }

        return Policy.Handle(canHandle).WaitAndRetryAsync(waits, (ex, wait) => { onRetryableFailure?.Invoke(ex); });
    }
}