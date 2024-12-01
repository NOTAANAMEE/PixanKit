using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.SystemInf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule.LibraryData;
using System.Xml.Linq;

namespace PixanKit.LaunchCore.GameModule.Game
{
    /// <summary>
    /// Modified Game. Game With ModLoader/ Optifine
    /// </summary>
    public class ModifiedGame: GameBase
    {
        /// <summary>
        /// Whether It Is useBaseGeneration Created
        /// </summary>
        protected bool useBaseGeneration;

        /// <summary>
        /// Init A <c>ModifiedGame</c> 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="jData"></param>
        public ModifiedGame(string path, JObject jData) : base(path, jData)
        {
            _gameType = GameType.Optifine;
            //InitJData(jData);
        }

        /// <summary>
        /// Init A Modified Game With Its Path
        /// </summary>
        /// <param name="path"></param>
        public ModifiedGame(string path):base(path, true)
        {
            _gameType = GameType.Optifine;
            //InitJData();
        }

        protected override void LoadJSON(JObject gameJdata)
        {
            if (!gameJdata.ContainsKey("inheritsFrom"))
            {
                useBaseGeneration = true;
                assetsID = gameJdata["assetIndex"]["id"].ToString();
            }
            else
            {
                _version = gameJdata["inheritsFrom"].ToString();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="folder"><inheritdoc/></param>
        public override void SetOwner(Folder folder)
        {
            base.SetOwner(folder);
            if (useBaseGeneration) return;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetCPArgs()
        {
            if (useBaseGeneration) return base.GetCPArgs();
            return base.SameVersionCPArgs() + base.GetCPArgs();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetGameArguments()
        {
            if (useBaseGeneration) return PCLGameArgProcess(base.GetGameArguments());
            return base.SameVersionGameArguments() + " " + base.GetGameArguments();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetJVMArguments()
        {
            if (useBaseGeneration) return base.GetJVMArguments();
            return base.GetJVMArguments();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        protected override string GetAssetsID()
        {
            if (useBaseGeneration) return base.GetAssetsID();
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
