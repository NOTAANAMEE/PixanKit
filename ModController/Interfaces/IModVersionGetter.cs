using Newtonsoft.Json.Linq;

namespace PixanKit.ModController.Interfaces
{
    /// <summary>
    /// I have no idea whether I should keep this?
    /// </summary>
    public interface IModVersionGetter
    {
        /// <summary>
        /// Really?
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task<JArray> GetVersionsAsync(CancellationToken token);
    }
}
