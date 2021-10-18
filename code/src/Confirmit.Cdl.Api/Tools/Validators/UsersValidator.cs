using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Tools.Database;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools.Validators
{
    public class UsersValidator : UsersValidatorBase<User>
    {
        private readonly IAccountLoader _accountLoader;
        private readonly QueryProvider _reader = new QueryProvider();

        public UsersValidator(IAccountLoader accountLoader)
        {
            _accountLoader = accountLoader;
        }

        protected override bool IsUserActive(User user)
        {
            return true;
        }

        protected override async Task<User[]> GetUsersInOrganizationAsync(User user)
        {
            return await _accountLoader.GetUsersInCompanyAsync(user.CompanyId);
        }

        protected override int GetUserId(User user)
        {
            return user.UserId;
        }

        protected override async Task<User> GetUserAsync(int id)
        {
            var user = await _accountLoader.GetUserAsync(id);
            if (user != null)
                return user;

            // User ID may be valid, but current principal does not have permissions to get it
            // Try to read user info from local database then
            return await ReadUserAsync(id);
        }

        private async Task<User> ReadUserAsync(int id)
        {
            var user = await _reader.GetUserAsync(id);
            if (user == null)
                return null;

            var company = await _reader.GetCompanyAsync(user.CompanyId);
            if (company == null)
                return null;

            user.CompanyName = company.Name;
            return user;
        }
    }
}