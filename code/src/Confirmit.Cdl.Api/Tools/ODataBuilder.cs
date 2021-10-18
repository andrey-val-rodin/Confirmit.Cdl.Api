using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;

namespace Confirmit.Cdl.Api.Tools
{
    public static class ODataBuilder
    {
        public static ODataQueryOptions<T> BuildOptions<T>(HttpRequest request)
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntityType(typeof(T));
            var edmModel = modelBuilder.GetEdmModel();
            var oDataQueryContext = new ODataQueryContext(edmModel, typeof(T), new ODataPath());

            return new ODataQueryOptions<T>(oDataQueryContext, request);
        }
    }
}
