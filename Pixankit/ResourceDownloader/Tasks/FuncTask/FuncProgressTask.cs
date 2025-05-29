namespace PixanKit.ResourceDownloader.Tasks.FuncTask;

/// <summary>
/// Represents a progress task that executes a user-defined function and tracks its progress.
/// </summary>
/// <typeparam name="T">The type of the result returned by the function.</typeparam>
public class FuncProgressTask<T> : ProgressTask
{
    /// <summary>
    /// Gets or sets the user-defined asynchronous function to execute.
    /// </summary>
    /// <remarks>
    /// The function accepts an <see cref="Action{Double}"/> for reporting progress and 
    /// a <see cref="CancellationToken"/> to handle cancellation.
    /// </remarks>
    public Func<Action<double>, CancellationToken, Task<T>>? Function;

    /// <summary>
    /// Gets the result returned by the executed function.
    /// </summary>
    public T? Return;

    /// <summary>
    /// Executes the user-defined function and handles progress reporting and exceptions.
    /// </summary>
    /// <remarks>
    /// This method invokes the <see cref="Function"/> asynchronously, passing the 
    /// <see cref="ProgressTask.ReportProgress(double)"/> method to allow progress updates and the 
    /// <see cref="CancellationToken"/> to handle cancellations. If an exception occurs, 
    /// it triggers the <see cref="ProgressTask.OnException"/> event.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the function is not defined (i.e., <see cref="Function"/> is null).
    /// </exception>
    protected override async Task Running()
    {
        try
        {
            if (Function != null)
                Return = await Function.Invoke(ReportProgress, CancellationToken.Token);
        }
        catch (Exception ex)
        {
            if (ex is not TaskCanceledException)
                ReportException(ex);
        }
        await base.Running();
    }
}