using PixanKit.ResourceDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.ResourceDownloader.Tasks.FuncTask
{

    public class FuncProgressTask<T>: ProgressTask
    {
        public Func<Action<double>, CancellationToken, Task<T>> Function;

        public T Return;

        protected override async Task Running()
        {
            try
            {
                Return = await Function(Report, CancellationToken.Token);
            }
            catch (Exception ex) 
            {
                OnException?.Invoke(ex);
            }
            await base.Running();
        }

    }
}
