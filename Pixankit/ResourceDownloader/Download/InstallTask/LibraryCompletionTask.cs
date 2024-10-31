using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.LaunchCore.Tasks;
using PixanKit.LaunchCore.Tasks.MultiTasks;
using PixanKit.LaunchCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.SystemInf;
using PixanKit.LaunchCore.Downloads;

namespace PixanKit.LaunchCore.Download.InstallTask
{
    public class LibraryCompletionTask:MultiAsyncTask
    {
        public static short ThreadNum = 64;

        GameBase? _game;

        public LibraryCompletionTask(GameBase game):base()
        {
            for (int i = 0; i < ThreadNum; i++)
            {
                Add(new MultiDownload());
            }
            _game = game;
            Init();
        }

        public LibraryCompletionTask()
        {
            for (int i = 0; i < ThreadNum; i++)
            {
                Add(new MultiDownload());
            }
        }

        public void SetGame(GameBase game)
        {
            _game = game;
            Init();
        }

        private void Init()
        {
            if (_game == null) throw new NullReferenceException();
            int ind = 0;
            foreach (var library in _game.libraries)
            {
                string path = _game.LibraryDir + '/' + library.Path;
                if (File.Exists(Localize.PathLocalize(path))) continue;
                (processes[ind % ThreadNum] as MultiDownload).Add(new DownloadTask(library.Url, path));
                ind++;
            }
        }
    }
}
