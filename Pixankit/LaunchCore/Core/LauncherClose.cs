using PixanKit.LaunchCore.Extention;
using PixanKit.LaunchCore.GameModule.Game;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Core
{
    public partial class Launcher
    {
        /// <summary>
        /// Close the launcher instance. It will also close the GameBase and Folders instances.
        /// </summary>
        public void Close()
        {
            Files.ModJData = SaveModData();
            Files.FolderJData = SaveFolderData();
            Files.RuntimeJData = SaveJavaData();
            Files.PlayerJData = SavePlayerData();
        }

        private JObject SaveModData()
        {
            JArray modinfs = new();
            foreach (var mod in Mods) 
            { 
                modinfs.Add(mod.ToJSON());
            }
            JObject jobj = new JObject();
            foreach (var folder in _folders) 
            {
                foreach (var game in folder.Games) if (game.GameType == GameModule.Game.GameType.Mod)
                {
                    jobj.Add(game.Path, ((ModloaderGame)game).ModToJSON());
                }
            }
            return new JObject()
            {
                { "children", modinfs},
                { "games", jobj} 
            };
        }

        private JObject SaveFolderData()
        {
            JArray folders = new();
            foreach (var folder in _folders) 
            {
                folders.Add(folder.ToJSON());
                folder.Close();
            }
            return new JObject()
            {
                { "children", folders},
                { "target", TargetGame.Path }
            };
        }

        private JObject SaveJavaData() 
        {
            JArray javaRuntimes = new();
            foreach (var javaRuntime in _javaRuntimes)
            {
                javaRuntimes.Add(javaRuntime.ToJSON());
            }
            return new JObject()
            {
                { "children", javaRuntimes}, 
            };
        }

        private JObject SavePlayerData()
        {
            JArray players = new();
            foreach (var player in _players)
            {
                players.Add(player.ToJSON());
            }
            return new JObject()
            {
                { "children", players},
                { "target", TargetPlayer.UID }
            };
        }
    }
}
