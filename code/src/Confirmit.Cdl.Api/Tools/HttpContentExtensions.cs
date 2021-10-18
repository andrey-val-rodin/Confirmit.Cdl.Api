using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools
{
    [PublicAPI]
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<T>(json);
            return value;
        }
    }
}