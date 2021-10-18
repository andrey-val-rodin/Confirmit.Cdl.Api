using Confirmit.Cdl.Api.Accounts;
using Confirmit.NetCore.Identity.Sdk.Claims;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools.Database
{
    public class DbRefresher
    {
        private readonly QueryProvider _reader = new QueryProvider();
        private readonly QueryProvider _queryProvider = new QueryProvider();
        private readonly IAccountLoader _accountLoader;
        private readonly ClaimsPrincipal _principal;

        public DbRefresher(IAccountLoader accountLoader, ClaimsPrincipal principal)
        {
            _accountLoader = accountLoader;
            _principal = principal;
        }

        public async Task RefreshAsync()
        {
            if (_principal.IsEndUser())
            {
                // We have to obtain enduser from endusers service because identity doesn't contain enduser company Id
                // Use trusted call because endusers service returns 401 Unauthorized for enduser account
                var enduser = await _accountLoader.GetEnduserAsync(_principal.UserId(), true);
                if (enduser == null)
                    return;

                // Get and refresh enduser list. We have to do this because principal doesn't contain enduser list name
                // Endusers service correctly returns own enduser list for enduser, so do not use trusted call
                var list = await _accountLoader.GetEnduserListAsync(enduser.ListId);
                if (list == null)
                    return;

                await RefreshEnduserListAsync(list);

                // Get enduser company and refresh it. Use trusted call
                var company = await _accountLoader.GetEnduserCompanyAsync(enduser.CompanyId, true);
                if (company == null)
                    return;

                await RefreshEnduserCompanyAsync(company);

                // Finally, refresh enduser
                await RefreshEnduserAsync(enduser).ConfigureAwait(false);
            }
            else
            {
                await RefreshCompanyAsync(CreateCompany()).ConfigureAwait(false);
                await RefreshUserAsync(CreateUser()).ConfigureAwait(false);
            }
        }

        private User CreateUser()
        {
            return new User
            {
                UserId = _principal.UserId(),
                UserName = _principal.UserName(),
                FirstName = _principal.FirstName(),
                LastName = _principal.LastName(),
                CompanyId = _principal.CompanyId()
            };
        }

        private Company CreateCompany()
        {
            return new Company
            {
                CompanyId = _principal.CompanyId(),
                Name = _principal.CompanyName()
            };
        }

        public async Task RefreshUserAsync(User user)
        {
            var userInDb = await _reader.GetUserAsync(user.UserId).ConfigureAwait(false);
            if (userInDb == null)
            {
                if (await _reader.GetCompanyAsync(user.CompanyId).ConfigureAwait(false) == null)
                    // Due to foreign constraint, we can’t append user without company
                    throw new InvalidOperationException("Attempt to refresh user without company");

                await _queryProvider.InsertUserAsync(user).ConfigureAwait(false);
            }
            else if (userInDb.UserName != user.UserName ||
                     userInDb.FirstName != user.FirstName ||
                     userInDb.LastName != user.LastName ||
                     userInDb.CompanyId != user.CompanyId)
            {
                await _queryProvider.UpdateUserAsync(user).ConfigureAwait(false);
            }
        }

        public async Task RefreshCompanyAsync(Company company)
        {
            var companyInDb = await _reader.GetCompanyAsync(company.CompanyId).ConfigureAwait(false);
            if (companyInDb == null)
                await _queryProvider.InsertCompanyAsync(company).ConfigureAwait(false);
            else if (companyInDb.Name != company.Name)
                await _queryProvider.UpdateCompanyAsync(company).ConfigureAwait(false);
        }

        public async Task RefreshEnduserAsync(Enduser enduser)
        {
            var enduserInDb = await _reader.GetEnduserAsync(enduser.Id).ConfigureAwait(false);
            if (enduserInDb == null)
            {
                if (await _reader.GetEnduserListAsync(enduser.ListId).ConfigureAwait(false) == null)
                    // Due to foreign constraint, we can’t append enduser without enduser list
                    throw new InvalidOperationException("Attempt to refresh enduser without enduser list");
                if (await _reader.GetEnduserCompanyAsync(enduser.CompanyId).ConfigureAwait(false) == null)
                    // Due to foreign constraint, we can’t append enduser without enduser company
                    throw new InvalidOperationException("Attempt to refresh enduser without enduser company");

                await _queryProvider.InsertEnduserAsync(enduser).ConfigureAwait(false);
            }
            else if (enduserInDb.Name != enduser.Name ||
                     enduserInDb.FirstName != enduser.FirstName ||
                     enduserInDb.LastName != enduser.LastName ||
                     enduserInDb.Email != enduser.Email ||
                     enduserInDb.ListId != enduser.ListId ||
                     enduserInDb.CompanyId != enduser.CompanyId)
            {
                await _queryProvider.UpdateEnduserAsync(enduser).ConfigureAwait(false);
            }
        }

        public async Task RefreshEnduserListAsync(EnduserList enduserList)
        {
            var enduserListInDb = await _reader.GetEnduserListAsync(enduserList.Id).ConfigureAwait(false);
            if (enduserListInDb == null)
                await _queryProvider.InsertEnduserListAsync(enduserList).ConfigureAwait(false);
            else if (enduserListInDb.Name != enduserList.Name)
                await _queryProvider.UpdateEnduserListAsync(enduserList).ConfigureAwait(false);
        }

        public async Task RefreshEnduserCompanyAsync(EnduserCompany company)
        {
            var companyInDb = await _reader.GetEnduserCompanyAsync(company.Id).ConfigureAwait(false);
            if (companyInDb == null)
                await _queryProvider.InsertEnduserCompanyAsync(company).ConfigureAwait(false);
            else if (companyInDb.Name != company.Name)
                await _queryProvider.UpdateEnduserCompanyAsync(company).ConfigureAwait(false);
        }
    }
}