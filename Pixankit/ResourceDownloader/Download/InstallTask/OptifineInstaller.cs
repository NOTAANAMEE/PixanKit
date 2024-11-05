using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixanKit.LaunchCore.GameModule;
using PixanKit.LaunchCore.GameModule.Game;
using PixanKit.ResourceDownloader.Tasks;
using PixanKit.ResourceDownloader.Tasks.MultiTasks;
using HtmlAgilityPack;

namespace PixanKit.ResourceDownloader.Download.InstallTask
{
    public class OptifineInstaller: MultiSequenceTask
    {
        //中文注释：
        /*
         *  本类需要实现的功能
         *  1.检测是否有该版本Minecraft
         *      若有 => 3
         *      若没有 => 2
         *  2.下载对应版本Minecraft并补全文件
         *  3.下载对应版本Optifine文件
         *  3.复制文件并解压内容读取
         *  4.生成cmd
         *  5.运行jar文件
         *  6.安装Minecraft 复制Minecraft文件 复制JSON文件 (选择性删除原Minecraft)
         */

        static Dictionary<string, string>? Optifines = null;

        string MCVersion = "";

        Folder Owner;

        string Name;

        /// <summary>
        /// Init an Optifine installer
        /// </summary>
        /// <param name="folder">The Target Minecraft Folder</param>
        /// <param name="name">The Actual Minecraft Name. The path will be folder\name\name.jar</param>
        /// <param name="optifineversion">The Optifine Version</param>
        public OptifineInstaller(Folder folder, string name, string optifineversion) 
        {
            Owner = folder;
            Name = name;
            string MCVersion = optifineversion
                [(optifineversion.IndexOf("-") + 1).. optifineversion.IndexOf('_')];
            //An optifine version example optifine-1.20.4_HD_U_L1 => optifine-${version}_...
            Init_CheckExists(folder);

        }

        /// <summary>
        /// This method will check whether version exists in this folder.
        /// If exists, skil. If not, add a Minecraft install task
        /// </summary>
        /// <param name="folder"></param>
        private void Init_CheckExists(Folder folder) 
        {
            if (folder.FindVersion(MCVersion, GameType.Ordinary) != null) return;
            Add(new OrdinaryInstallTask(folder, MCVersion, MCVersion));
        }

        private void Init_DownloadOptifine(string optifineVersion)
        {
            
        }

        public static async Task<Dictionary<string, string>> GetOptifineVersion() 
        {
            using HttpClient client = new() { BaseAddress = new Uri("https://optifine.net") };
            var response = await client.GetAsync("/downloads");
            HtmlDocument document = new();
            document.Load(response.Content.ReadAsStream());
            
            return null;
        }
    }
}
