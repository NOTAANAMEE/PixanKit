namespace PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

/// <summary>
/// Represents a progress task that executes all its sub-tasks asynchronously and concurrently.
/// </summary>
public class AsyncProgressTask : MultiProgressTask
{
    /// <summary>
    /// Starts all sub-tasks concurrently and then starts the main task.
    /// </summary>
    /// <remarks>
    /// This method ensures that each task in the <see cref="ProgressTask"/> list is started 
    /// before the main task begins its execution. Tasks are executed asynchronously, 
    /// allowing them to run concurrently.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the task has already been started or is in a state other than <see cref="ProgressStatus.Inited"/>.
    /// </exception>
    public override void Start()
    {
        if (_status == ProgressStatus.Inited)
            foreach (var item in ProgressTasks)
            {
                item.Start();
            }
        base.Start();
    }
}

/// <summary>
/// Represents a progress task that executes all its sub-tasks asynchronously and concurrently.
/// </summary>
public class AsyncProgressTask<T> : MultiProgressTask<T> where T : ProgressTask
{
    /// <summary>
    /// Starts all sub-tasks concurrently and then starts the main task.
    /// </summary>
    /// <remarks>
    /// This method ensures that each task in the <see cref="ProgressTask"/> list is started 
    /// before the main task begins its execution. Tasks are executed asynchronously, 
    /// allowing them to run concurrently.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the task has already been started or is in a state other than <see cref="ProgressStatus.Inited"/>.
    /// </exception>
    public override void Start()
    {
        if (_status == ProgressStatus.Inited)
            foreach (var item in ProgressTasks)
            {
                item.Start();
            }
        base.Start();
    }
}