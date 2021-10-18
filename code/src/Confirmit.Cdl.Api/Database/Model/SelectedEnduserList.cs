using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.Database.Model
{
    [PublicAPI]
    public class SelectedEnduserList
    {
        public long DocumentId { get; set; }

        public int ListId { get; set; }
    }
}
