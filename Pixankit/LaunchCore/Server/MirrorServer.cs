using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.Server
{
    /// <summary>
    /// Mirror Server
    /// </summary>
    public class MirrorServer
    {
        protected string OriginURL;

        public string BaseURL;

        /// <summary>
        /// Initor But Do Nothing. Do Not Use That
        /// </summary>
        public MirrorServer() { OriginURL = ""; BaseURL = ""; }

        ///<summary>Initor</summary>
        /// <param name="OriginURL">The Url Part That Needs To Replace(If No, Make It "")</param>
        /// <param name="ReplacedURL">The Url That Replacing(Include https://)</param>
        public MirrorServer(string OriginURL, string ReplacedURL)
        {
            this.OriginURL = OriginURL;
            this.BaseURL = ReplacedURL;
        }

        /// <summary>
        /// Replace The URL
        /// </summary>
        /// <param name="url">The Origin URL</param>
        /// <returns>The Replaced URL</returns>
        public virtual string Replace(string url)
        {
            return OriginURL == "" ? url : url.Replace(OriginURL, BaseURL);
        }
    }
}
