using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Mod;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.LibraryData;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// Modified Game. Game With ModLoader/ Optifine
    /// </summary>
    public class ModifiedGame: GameBase
    {
        /// <summary>
        /// Whether It Is PCL2 Created
        /// </summary>
        protected bool PCL2;

        /// <summary>
        /// Init A <c>ModifiedGame</c> 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="jData"></param>
        public ModifiedGame(string path, JObject jData) : base(path, jData)
        {
            _gameType = GameType.Optifine;
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="folder"><inheritdoc/></param>
        public override void SetOwner(Folder folder)
        {
            base.SetOwner(folder);
            if (PCL2) return;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetCPArgs()
        {
            return base.SameVersionCPArgs() + base.GetCPArgs();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetGameArguments()
        {
            if (PCL2) return PCLGameArgProcess(base.GetGameArguments());
            return base.SameVersionGameArguments() + " " + base.GetGameArguments();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetJVMArguments()
        {
            if (PCL2) return base.GetJVMArguments();
            return base.GetJVMArguments();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetAssetsID()
        {
            if (PCL2) return base.GetAssetsID();
            return base.SameVersionAssetsID();
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
