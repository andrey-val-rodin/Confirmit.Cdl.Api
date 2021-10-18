using Confirmit.Cdl.Api.Authorization;
using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class PermissionDto
    {
        public int Id { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public class UserPermissionDto
    {
        public int Id { get; set; }
        public string UserKey { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public sealed class UserPermissionFullDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public sealed class EnduserPermissionFullDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int EnduserListId { get; set; }
        public string EnduserListName { get; set; }
        public int EnduserCompanyId { get; set; }
        public string EnduserCompanyName { get; set; }
        public Permission Permission { get; set; }
    }

    [PublicAPI]
    public class OrganizationPermissionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Permission Permission { get; set; }
    }
}