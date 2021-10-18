using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Tools.Database;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools.Validators
{
    public class EndusersValidator : UsersValidatorBase<Enduser>
    {
        private readonly IAccountLoader _accountLoader;
        private readonly QueryProvider _reader = new QueryProvider();

        public EndusersValidator(IAccountLoader accountLoader)
        {
            _accountLoader = accountLoader;
        }

        protected override bool IsUserActive(Enduser enduser)
        {
            return enduser == null || enduser.IsActive;
        }

        protected override async Task<Enduser[]> GetUsersInOrganizationAsync(Enduser enduser)
        {
            return await _accountLoader.GetEndusersInListAsync(enduser.ListId);
        }

        protected override int GetUserId(Enduser enduser)
        {
            return enduser.Id;
        }

        protected override async Task<Enduser> GetUserAsync(int id)
        {
            var enduser = await _accountLoader.GetEnduserAsync(id);
            if (enduser != null)
                return enduser;

            // Enduser ID may be valid, but current principal does not have permissions to get it
            // Try to read enduser info from local database then
            return await ReadEnduserAsync(id);
        }

        private async Task<Enduser> ReadEnduserAsync(int id)
        {
            return await _reader.GetEnduserAsync(id);
        }
    }
}