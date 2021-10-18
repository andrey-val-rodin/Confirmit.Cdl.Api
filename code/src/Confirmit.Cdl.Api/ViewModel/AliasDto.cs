using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class AliasToCreateDto
    {
        public string Namespace { get; set; }
        public string Alias { get; set; }
        public long DocumentId { get; set; }
    }

    [PublicAPI]
    public class AliasPatchDto
    {
        public long DocumentId { get; set; }
    }

    [PublicAPI]
    public sealed class AliasDto
    {
        public long Id { get; set; }
        public string Namespace { get; set; }
        public string Alias { get; set; }
        public long DocumentId { get; set; }
        public JObject Links { get; set; }
    }
}