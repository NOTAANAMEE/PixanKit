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
        /// <summary>
        /// The original base url of the network assets
        /// </summary>
        protected string OriginalURL;

        /// <summary>
        /// The base url of the network assets. It will replace the base url
        /// </summary>
        public string BaseURL;

        /// <summary>
        /// Initor But Do Nothing. Do Not Use That
        /// </summary>
        public MirrorServer() { OriginalURL = ""; BaseURL = ""; }

        ///<summary>Initor</summary>
        /// <param name="OriginalURL">The Url Part That Needs To Replace(If No, Make It "")</param>
        /// <param name="ReplacedURL">The Url That Replacing(Include https://)</param>
        public MirrorServer(string OriginalURL, string ReplacedURL)
        {
            this.OriginalURL = OriginalURL;
            this.BaseURL = ReplacedURL;
        }

        /// <summary>
        /// Replace The URL
        /// </summary>
        /// <param name="url">The Original URL</param>
        /// <returns>The Replaced URL</returns>
        public virtual string Replace(string url)
        {
            return OriginalURL == "" ? url : url.Replace(OriginalURL, BaseURL);
        }
    }
}
