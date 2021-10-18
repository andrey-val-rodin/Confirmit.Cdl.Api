using JetBrains.Annotations;
using System.Collections.Generic;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class PageLinks
    {
        public string PreviousPage { get; set; }
        public string NextPage { get; set; }
    }

    [PublicAPI]
    public class PageDto<T>
    {
        public int ItemCount => Items.Count;
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
        public PageLinks Links { get; set; }
    }
}