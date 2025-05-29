namespace PixanKit.ResourceDownloader.Tasks.MultiProgressTask;

/// <summary>
/// Represents a task that combines and tracks multiple progress tasks.
/// </summary>
public class MultiProgressTask : ProgressTask
{
    /// <summary>
    /// Gets the list of progress tasks managed by this multi-progress task.
    /// </summary>
    public List<ProgressTask> ProgressTasks = [];

    /// <summary>
    /// Adds a progress task to the multi-progress task.
    /// </summary>
    /// <param name="task">The progress task to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the multi-progress task has already started.
    /// </exception>
    public void Add(ProgressTask task)
    {
        if (_status > ProgressStatus.Inited)
            throw new InvalidOperationException("Add A Process Before The Task Starts");
        ProgressTasks.Add(task);
        Tasks.Add(task.MainTask);
        task.OnException += ReportException;
        task.OnReport += ReportProgress;
    }

    /// <summary>
    /// Cancels all tasks within the multi-progress task.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a task has already been canceled.
    /// </exception>
    public override void Cancel()
    {
        foreach (var progressTask in ProgressTasks)
        {
            if (progressTask.Status < ProgressStatus.Canceled) progressTask.Cancel();
        }
        base.Cancel();
    }
    
    /// <summary>
    /// Reports the combined progress of all tasks managed by this multi-progress task.
    /// </summary>
    /// <param name="progress">The combined progress of all sub-tasks.</param>
    protected override void ReportProgress(double progress)
    {
        progress = 0;
        foreach (var task in ProgressTasks)
        {
            progress += task.Progress;
        }
        progress /= ProgressTasks.Count;
        base.ReportProgress(progress);
    }
}

/// <summary>
/// Represents a task that combines and tracks multiple progress tasks.
/// </summary>
public class MultiProgressTask<T> : ProgressTask where T : ProgressTask
{
    /// <summary>
    /// Gets the list of progress tasks managed by this multi-progress task.
    /// </summary>
    public List<T> ProgressTasks = [];

    /// <summary>
    /// Adds a progress task to the multi-progress task.
    /// </summary>
    /// <param name="task">The progress task to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the multi-progress task has already started.
    /// </exception>
    public void Add(T task)
    {
        if (_status > ProgressStatus.Inited)
            throw new InvalidOperationException("Add A Process Before The Task Starts");
        ProgressTasks.Add(task);
        Tasks.Add(task.MainTask);
        task.OnException += ReportException;
        task.OnReport += ReportProgress;
    }

    /// <summary>
    /// Cancels all tasks within the multi-progress task.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if a task has already been canceled.
    /// </exception>
    public override void Cancel()
    {
        foreach (var progressTask in ProgressTasks)
        {
            if (progressTask.Status < ProgressStatus.Canceled) progressTask.Cancel();
        }
        base.Cancel();
    }

    /// <summary>
    /// Reports the combined progress of all tasks managed by this multi-progress task.
    /// </summary>
    /// <param name="progress">The combined progress of all sub-tasks.</param>
    protected override void ReportProgress(double progress)
    {
        progress = 0;
        foreach (var task in ProgressTasks)
        {
            progress += task.Progress;
        }
        progress /= ProgressTasks.Count;
        base.ReportProgress(progress);
    }
}