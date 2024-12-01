using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server
{
    /// <summary>
    /// Resource Server Base
    /// </summary>
    public abstract class ResourceServer
    {
        /// <summary>
        /// The Mirror Servers That Available
        /// </summary>
        public List<MirrorServer> Mirrors = new();

        /// <summary>
        /// Current Mirror Server
        /// </summary>
        public MirrorServer Current;

        /// <summary>
        /// Choose The Best MirrorServer From The List
        /// </summary>
        public void UpdateIndex()
        {
            List<KeyValuePair<long, MirrorServer>> dict = new();
            foreach (var item in Mirrors)
            {
                long pingtime = GetPing(item.BaseURL);
                if (pingtime > 0)
                    dict.Add(new(pingtime, item));
            }
            dict.Sort((a, b) => a.Key.CompareTo(b.Key));
            Current = dict.First().Value;
        }

        /// <summary>
        /// Specify A Current Server From The List
        /// </summary>
        /// <param name="index">The index Of The Server In The List</param>
        public void UpdateIndex(int index)
        {
            Current = Mirrors[index];
        }

        /// <summary>
        /// Get The Time That Used To Ping The Current Server
        /// </summary>
        /// <returns>The Time Needs To Ping</returns>
        public long Ping()
        {
            return GetPing(Current.BaseURL);
        }

        /// <summary>
        /// Get The Time That Used To Ping The Server
        /// </summary>
        /// <param name="url">The URL  The Method Will Automatically 
        /// Recognize The Server</param>
        /// <returns>The Time Needs To Ping</returns>
        public static long GetPing(string url)
        {
            Ping ping = new();
            var reply = ping.Send(GetHost(url));
            if (reply.Status == IPStatus.Success) return reply.RoundtripTime;
            else return -1;
        }

        /// <summary>
        /// Add A Mirror Server
        /// </summary>
        /// <param name="server">The Mirror Server</param>
        public void Add(MirrorServer server)
        {
            Mirrors.Add(server);
        }

        /// <summary>
        /// Replace The OriginalUrl From The Server
        /// </summary>
        /// <param name="OriginalUrl">The Original Url For The Resource</param>
        /// <returns>The URL Of The Resource On The Current MirrorServer</returns>
        protected string Replace(string OriginalUrl) => Current.Replace(OriginalUrl);

        private static string GetHost(string OriginalUrl)
        {
            Uri uri = new(OriginalUrl);
            return uri.Host;
        }
    }
}
