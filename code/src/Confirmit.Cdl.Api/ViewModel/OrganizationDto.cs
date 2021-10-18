using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class OrganizationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}