using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks
{
    public abstract class ProgressTask
    {
        public Action<double> OnReport;

        public Action<ProgressTask> OnStart;

        public Action<ProgressTask> OnCancel;

        public Action<ProgressTask> OnFinish;

        public Action<Exception> OnException;

        public double Progress { get => _progress; }

        public Task MainTask { get => TaskStopped.Task;}

        public ProgressStatus Status { get => _status; }

        protected CancellationTokenSource CancellationToken = new();

        protected TaskCompletionSource TaskStopped = new();

        protected double _progress;

        protected List<Task> _tasks = new();

        protected ProgressStatus _status;

        protected object _lock = new();

        public ProgressTask() { _status = ProgressStatus.Inited; }

        public virtual void Start()
        {
            if (_status != ProgressStatus.Inited) 
                throw new InvalidOperationException("Do Not Start Twice");
            _status = ProgressStatus.Running;
            _ = Running();
        }

        public virtual void Cancel() 
        {
            if (CancellationToken.IsCancellationRequested) 
                throw new InvalidOperationException("Do Not Cancel Twice");
            CancellationToken.Cancel();
            OnCancel?.Invoke(this);
            _status = ProgressStatus.Canceled;
        }

        protected virtual async Task Running()
        {
            await Task.WhenAll(_tasks);

            if (!CancellationToken.IsCancellationRequested) Finish();
            TaskStopped.SetResult();
        }

        protected virtual void Finish()
        {
            OnFinish?.Invoke(this);
            _status = ProgressStatus.Finished;
        }

        protected virtual void Report(double progress) 
        {
            lock (_lock)
            {
                _progress = progress;
                OnReport?.Invoke(progress);
            }
        }
    }

    public enum ProgressStatus
    {
        Initing,
        Inited,
        Running,
        Canceled,
        Finished
    }
}
