using PixanKit.LaunchCore.Logger;

namespace PixanKit.ResourceDownloader.Tasks;

/// <summary>
/// This class defines a task base class for tracking progress, 
/// supporting callback mechanisms for progress updates, task start, 
/// cancellation, and completion
/// </summary>
public abstract class ProgressTask
{
    /// <summary>
    /// Progress Make Event, Call When Progress Is Updated
    /// </summary>
    public Action<double>? OnReport;

    /// <summary>
    /// Start Event, Call When Task Starts
    /// </summary>
    public Action<ProgressTask>? OnStart;

    /// <summary>
    /// Cancel Event, Call When Task Is Cancelled
    /// </summary>
    public Action<ProgressTask>? OnCancel;

    /// <summary>
    /// Finish Event, Call When Task Is Finished
    /// </summary>
    public Action<ProgressTask>? OnFinish;

    /// <summary>
    /// Exception Event, Not Using...
    /// </summary>
    public Action<Exception>? OnException;

    /// <summary>
    /// Store Current Progress
    /// </summary>
    public double Progress => _progress;

    /// <summary>
    /// The Task corresponding to the current task,
    /// will wait for the task to be completed or cancelled
    /// </summary>
    public Task MainTask => TaskStopped.Task;

    /// <summary>
    /// The status of the current task
    /// </summary>
    public ProgressStatus Status => _status;

    /// <summary>
    /// The CancellationTokenSource used to signal task cancellation.
    /// </summary>
    protected CancellationTokenSource CancellationToken = new();

    /// <summary>
    /// The TaskCompletionSource used to signal the completion of the task.
    /// </summary>
    protected TaskCompletionSource TaskStopped = new();

    /// <summary>
    /// The progress of the current task.
    /// </summary>
    protected double _progress;

    /// <summary>
    /// The subtasks of the current task.
    /// </summary>
    protected List<Task> Tasks = [];

    /// <summary>
    /// The status of the current task.
    /// </summary>
    protected ProgressStatus _status;

    /// <summary>
    /// Lock object for progress reporting.
    /// </summary>
    protected Lock _progressLock = new();

    /// <summary>
    /// Constructor, initialize the task status to ProgressStatus.Initialized
    /// </summary>
    protected ProgressTask() { _status = ProgressStatus.Inited; }

    /// <summary>
    /// Starts the task
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// When the status of the task is not <c>ProgressStatus.Inited</c>
    /// the exception will be thrown
    /// </exception>
    public virtual void Start()
    {
        if (_status != ProgressStatus.Inited)
            throw new InvalidOperationException("Do Not Start Twice");
        _status = ProgressStatus.Running;
        _ = Running();
    }

    /// <summary>
    /// Cancel the task
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// If the task has been canceled, an InvalidOperationException will be thrown</exception>
    public virtual void Cancel()
    {
        if (CancellationToken.IsCancellationRequested)
            throw new InvalidOperationException("Do Not Cancel Twice");
        CancellationToken.Cancel();
        OnCancel?.Invoke(this);
        _status = ProgressStatus.Canceled;
    }

    /// <summary>
    /// Reports an exception that occurs during the task's execution.
    /// </summary>
    /// <param name="ex">The exception to report.</param>
    /// <remarks>
    /// This method cancels the task and triggers the <see cref="OnException"/> event 
    /// to notify listeners about the exception.
    /// </remarks>
    public virtual void ReportException(Exception ex)
    {
        Cancel();
        OnException?.Invoke(ex);
    }

    /// <summary>
    /// Execute the main logic of the task.<br/>
    /// Wait for all subtasks to complete. If the task is not cancelled, 
    /// call the Finish() method. Finally, set the task completion mark.
    /// </summary>
    /// 
    protected virtual async Task Running()
    {
        await Task.WhenAll(Tasks);

        if (!CancellationToken.IsCancellationRequested) Finish();
        TaskStopped.SetResult();
    }

    /// <summary>
    /// The callback logic when the task is completed triggers 
    /// the OnFinish event and updates the status to ProgressStatus.Finished
    /// </summary>
    protected virtual void Finish()
    {
        try
        {
            OnFinish?.Invoke(this);
        }
        catch (Exception ex)
        {
            Logger.Warn("PixanKit.ResourceDownloader", ex.ToString());
            Logger.Warn("PixanKit.ResourceDownloader", ex.StackTrace ?? "");
        }
        ReportProgress(1);
        _status = ProgressStatus.Finished;
    }

    /// <summary>
    /// Reports the progress of the current task.
    /// </summary>
    /// <param name="progress">current progress value (double)</param>
    protected virtual void ReportProgress(double progress)
    {
        lock (_progressLock)
        {
            _progress = progress;
            OnReport?.Invoke(progress);
        }
    }
}

/// <summary>
/// Task status enumeration.
/// </summary>
public enum ProgressStatus
{
    /// <summary>
    /// Initializing
    /// </summary>
    Initing,
    /// <summary>
    /// Initialization completed, task not started
    /// </summary>
    Inited,
    /// <summary>
    /// Task running
    /// </summary>
    Running,
    /// <summary>
    /// Task canceled
    /// </summary>
    Canceled,
    /// <summary>
    /// Task completed
    /// </summary>
    Finished
}