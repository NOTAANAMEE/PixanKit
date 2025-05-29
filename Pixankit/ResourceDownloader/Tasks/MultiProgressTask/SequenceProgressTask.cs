namespace PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

/// <summary>
/// Represents a sequence of progress tasks that are executed one after another.
/// </summary>
public class SequenceProgressTask : MultiProgressTask
{
    /// <summary>
    /// Executes all progress tasks in the sequence one by one.
    /// </summary>
    /// <remarks>
    /// Each task in the <see cref="ProgressTask"/> list will start sequentially. 
    /// If a cancellation is requested, the sequence will stop execution.
    /// After all tasks are completed or the sequence is canceled, 
    /// the base <see cref="Running"/> logic will finalize the task.
    /// </remarks>
    protected override async Task Running()
    {
        foreach (var progresstask in ProgressTasks)
        {
            if (CancellationToken.Token.IsCancellationRequested) break;
            progresstask.Start();
            await progresstask.MainTask;
        }
        await base.Running();
    }
}

/// <summary>
/// Represents a sequence of progress tasks that are executed one after another.
/// </summary>
public class SequenceProgressTask<T> : MultiProgressTask<T> where T : ProgressTask
{
    /// <summary>
    /// Executes all progress tasks in the sequence one by one.
    /// </summary>
    /// <remarks>
    /// Each task in the <see cref="ProgressTask"/> list will start sequentially. 
    /// If a cancellation is requested, the sequence will stop execution.
    /// After all tasks are completed or the sequence is canceled, 
    /// the base <see cref="Running"/> logic will finalize the task.
    /// </remarks>
    protected override async Task Running()
    {
        foreach (var progresstask in ProgressTasks)
        {
            if (CancellationToken.Token.IsCancellationRequested) break;
            progresstask.Start();
            await progresstask.MainTask;
        }
        await base.Running();
    }
}