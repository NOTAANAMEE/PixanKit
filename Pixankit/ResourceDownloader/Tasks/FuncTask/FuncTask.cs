using PixanKit.ResourceDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.FuncTask
{
    /// <summary>
    /// Trackable Task Method Without Return
    /// </summary>
    /// <param name="process">Ref Double Schedule</param>
    /// <param name="cancelRequest">Cancel Request
    /// <br/>Please Check Regularly
    /// </param>
    /// <returns>NULL</returns>
    public delegate Task TrackableTaskAction(TrackActionTask process, 
        CancellationToken cancelRequest);

    /// <summary>
    /// Trackable Task Method With Return
    /// </summary>
    /// <param name="process">Change Schedule With process</param>
    /// <param name="cancelRequest">Cancel Request
    /// <br/>Please Check Regularly
    /// </param>
    /// <typeparam name="T">Type T</typeparam>
    /// <returns>Return Of The Method</returns>
    public delegate Task<T> TrackableTaskFunc<T>(TrackFuncTask<T> process, 
        CancellationToken cancelRequest);

    /// <summary>
    /// Trackable Method Without Return
    /// </summary>
    public class TrackActionTask:ProcessTask
    {
        /// <summary>
        /// The Methods
        /// </summary>
        public TrackableTaskAction? Action;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override double Schedule => Sched;

        /// <summary>
        /// Outside Set Schedule
        /// </summary>
        public double Sched = 0;

        CancellationTokenSource canceltoken = new();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override Task Cancel()
        {
            canceltoken.Cancel();
            return base.Cancel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override async Task Running()
        {
            if (Action != null)
            await Action(this, canceltoken.Token);
            await base.Running();
        }
    }

    /// <summary>
    /// Trackable Functions With Returns
    /// </summary>
    /// <typeparam name="T">Type Of The Return</typeparam>
    public class TrackFuncTask<T>:ProcessTask
    {
        /// <summary>
        /// Function Return
        /// </summary>
        public T? Return;

        /// <summary>
        /// The Delegate
        /// </summary>
        public TrackableTaskFunc<T>? Function;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override double Schedule => Sched;

        /// <summary>
        /// Outside Set Schedule
        /// </summary>
        public double Sched = 0;

        CancellationTokenSource canceltoken = new();

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override Task Cancel()
        {
            canceltoken.Cancel();
            return base.Cancel();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override async Task Running()
        {
            if (Function != null)
            Return = await Function(this, canceltoken.Token);
            await base.Running();
        }

    }
}
