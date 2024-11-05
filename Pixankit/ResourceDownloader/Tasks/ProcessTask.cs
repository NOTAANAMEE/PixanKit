using System.Net;

namespace PixanKit.ResourceDownloader.Tasks
{
    /// <summary>
    /// This is the process task of this library.
    /// asynchronously download and install files and track it.
    /// </summary>
    public abstract class ProcessTask
    {
        /// <summary>
        /// The List Of The Tasks
        /// </summary>
        protected List<Task> Tasks = new();

        //protected TaskCompletionSource<bool> Starting = new TaskCompletionSource<bool>(false);
        /// <summary>
        /// The Process Is Finished
        /// </summary>
        protected TaskCompletionSource<bool> Finishing = new(false);

        /// <summary>
        /// The Process Is Not Running
        /// </summary>
        protected TaskCompletionSource<bool> TaskStopped = new(false);

        /// <summary>
        /// This action will be called when the process is started
        /// </summary>
        public Action? OnStart;

        /// <summary>
        /// This action will be called when the process finishes
        /// </summary>
        public Action? OnFinish;

        /// <summary>
        /// This action will be called when the process is canceled
        /// </summary>
        public Action? OnCancel;

        /// <summary>
        /// Checks whether the process is running
        /// </summary>
        public ProcessStatus Status
        { get => _status; }

        /// <summary>
        /// Current Status
        /// </summary>
        protected ProcessStatus _status = ProcessStatus.Initing;
        
        /// <summary>
        /// Wait the process until it finish or canceled
        /// </summary>
        public Task MainTask
        {
            get => Finishing.Task;
        }
        
        /// <summary>
        /// Judge whether the proccess is running
        /// </summary>
        public bool ProcessRunning { get => Status == ProcessStatus.Running; }

        /// <summary>
        /// To trace the prgress.
        /// Be careful! full value is not 1 but 10
        /// </summary>
        public virtual double Schedule
        {
            get
            {
                int f = 0;
                foreach (var task in Tasks) if (task.IsCompleted) f++;
                return f / (double)Tasks.Count * 10;
            }
        }

        /// <summary>
        /// Protected Initor
        /// </summary>
        protected ProcessTask() { }

        /// <summary>
        /// This method starts the process
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual void Start()
        {
            if (Status != ProcessStatus.Initing) throw new InvalidOperationException();
            OnStart?.Invoke();
            //Starting.SetResult(true);
            _ = Running();
            _status = ProcessStatus.Running;
        }

        /// <summary>
        /// This method cancels the process
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async void Cancel()
        {
            if (_status >= ProcessStatus.Canceling) throw new InvalidOperationException();
            _status = ProcessStatus.Canceling;
            if (_status == ProcessStatus.Running) await TaskStopped.Task;
            OnCancel?.Invoke();
            _status = ProcessStatus.Canceled;
            Finishing.SetResult(true);
        }

        /// <summary>
        /// This method will be called if the process is finished
        /// </summary>
        protected virtual void Finish()
        {
            OnFinish?.Invoke();
            _status = ProcessStatus.Finished;
            Finishing.SetResult(true);
        }

        /// <summary>
        /// This method will be called if the process is started
        /// </summary>
        /// <returns></returns>
        protected virtual async Task Running()
        {
            foreach (var task in Tasks)
            {
                await task;
                if (Status != ProcessStatus.Running) break;
            }
            TaskStopped.SetResult(true);
            if (Status == ProcessStatus.Running) Finish();
        }
    }

    /// <summary>
    /// Initing: Not started yet
    /// Running: Running now
    /// Canceled: Canceled by outside programs
    /// Finished: Process finished
    /// </summary>
    public enum ProcessStatus
    {
        Initing,
        Running,
        Canceling,
        Canceled,
        Finished
    }
}
