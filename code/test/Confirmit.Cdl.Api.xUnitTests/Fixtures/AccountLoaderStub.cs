using Confirmit.Cdl.Api.Accounts;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xUnitTests.Fixtures
{
    public sealed class AccountLoaderStub : IAccountLoader
    {
        private int[] Companies { get; }

        public AccountLoaderStub(int[] companies = null)
        {
            Companies = companies;
        }

        public Task<User> GetUserAsync(int userId)
        {
            return Task.FromResult(new User { UserId = userId });
        }

        public Task<User> GetUserAsync(string userKey)
        {
            return GetUserAsync(1);
        }

        public Task<Company> GetCompanyAsync(int companyId)
        {
            return Task.FromResult(new Company { CompanyId = companyId });
        }

        public Task<Company[]> GetAllCompaniesAsync(CompanyPermissionType permission)
        {
            return Companies == null
                ? Task.FromResult(new Company[] { })
                : Task.FromResult(Companies.Select(c => new Company { CompanyId = c }).ToArray());
        }

        public Task<User[]> GetUsersInCompanyAsync(int companyId)
        {
            return Task.FromResult(new User[] { });
        }

        public Task<Enduser> GetEnduserAsync(int enduserId, bool useTrustedClaim = false)
        {
            return Task.FromResult(new Enduser { Id = enduserId });
        }

        public Task<EnduserList> GetEnduserListAsync(int listId)
        {
            return Task.FromResult(new EnduserList { Id = listId });
        }

        public Task<EnduserList[]> GetManyEnduserListsAsync(int[] ids)
        {
            return Task.FromResult(ids.Select(l => new EnduserList { Id = l }).ToArray());
        }

        public Task<Enduser[]> GetEndusersInListAsync(int listId)
        {
            return Task.FromResult(new Enduser[] { });
        }

        public Task<EnduserCompany> GetEnduserCompanyAsync(int companyId, bool useTrustedClaim)
        {
            return Task.FromResult(new EnduserCompany());
        }

        public Task<EnduserCompany[]> GetEnduserListCompaniesAsync(int listId)
        {
            return Task.FromResult(new EnduserCompany[] { });
        }
    }
}
