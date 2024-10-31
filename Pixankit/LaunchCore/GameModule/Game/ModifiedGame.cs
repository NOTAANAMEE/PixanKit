using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Mod;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Game
{
    public class ModifiedGame: GameBase
    {
        protected bool PCL2;

        public ModifiedGame(string path, JObject jData) : base(jData)
        {
            _gameType = GameType.Mod;
            _path = path;
            if (!jData.ContainsKey("inheritsFrom"))
            {
                PCL2 = true;
                assetsID = jData["assetIndex"]["id"].ToString();
            }
            else
            {
                _version = jData["inheritsFrom"].ToString();
            }
        }

        public override void SetOwner(Folder folder)
        {
            base.SetOwner(folder);
            if (PCL2) return;
        }

        internal override string GetCPArgs()
        {
            if (PCL2) return base.GetCPArgs();
            else
            {
                return Owner.FindGame(Version).GetCPArgs() + Localize.LocalParser + base.GetCPArgs();
            }
        }

        internal override string GetGameArguments()
        {
            if (PCL2) return PCLGameArgProcess(base.GetGameArguments());
            return Owner.FindGame(Version).GetGameArguments() + " " + base.GetGameArguments();
        }

        internal override string GetJVMArguments()
        {
            if (PCL2) return base.GetJVMArguments();
            return base.GetJVMArguments();
        }

        internal override string GetAssetsID()
        {
            if (PCL2) return base.GetAssetsID();
            return Owner.FindGame(Version).GetAssetsID();
        }

        internal string PCLGameArgProcess(string args)
        {
            if (args.Contains(" --tweakClass optifine.OptiFineForgeTweaker"))
            {
                args = args.Replace(" --tweakClass optifine.OptiFineForgeTweaker", "");
                args += " --tweakClass optifine.OptiFineForgeTweaker";
            }
            return args;
        }
    }
}
