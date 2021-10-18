using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class RootDto
    {
        public string Id => "Confirmit.Cdl.Api";
        public JObject Links { get; set; }
    }
}