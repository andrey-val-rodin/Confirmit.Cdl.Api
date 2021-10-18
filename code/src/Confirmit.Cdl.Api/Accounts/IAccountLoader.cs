using JetBrains.Annotations;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Accounts
{
    [PublicAPI]
    public enum CompanyPermissionType
    {
        Read = 1,
        Write = 2,
        Delete = 3,
        AdministratorRole = 137
    }

    /// <summary>
    /// Represents methods to load accounts from external REST-services
    /// </summary>
    [PublicAPI]
    public interface IAccountLoader
    {
        Task<User> GetUserAsync(int userId);
        Task<User> GetUserAsync(string userKey);
        Task<Company> GetCompanyAsync(int companyId);
        Task<Company[]> GetAllCompaniesAsync(CompanyPermissionType permission);
        Task<User[]> GetUsersInCompanyAsync(int companyId);
        Task<Enduser> GetEnduserAsync(int enduserId, bool useTrustedClaim = false);
        Task<EnduserList> GetEnduserListAsync(int listId);
        Task<EnduserList[]> GetManyEnduserListsAsync(int[] ids);
        Task<Enduser[]> GetEndusersInListAsync(int listId);
        Task<EnduserCompany> GetEnduserCompanyAsync(int companyId, bool useTrustedClaim = false);
        Task<EnduserCompany[]> GetEnduserListCompaniesAsync(int listId);
    }
}