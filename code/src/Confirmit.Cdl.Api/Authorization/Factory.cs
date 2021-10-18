using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization.Users;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.NetCore.Identity.Sdk.Claims;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using Enduser = Confirmit.Cdl.Api.Authorization.Users.Enduser;
using User = Confirmit.Cdl.Api.Authorization.Users.User;

namespace Confirmit.Cdl.Api.Authorization
{
    public sealed class Factory
    {
        public Factory(CdlDbContext dbContext, IHttpContextAccessor httpContext, IAccountLoader accountLoader)
            : this(dbContext, httpContext.HttpContext.User, accountLoader)
        {
        }

        public Factory(CdlDbContext dbContext, ClaimsPrincipal principal, IAccountLoader accountLoader)
        {
            DbContext = dbContext;
            Principal = principal;
            AccountLoader = accountLoader;
        }

        private CdlDbContext DbContext { get; }
        private IAccountLoader AccountLoader { get; }
        private ClaimsPrincipal Principal { get; }

        public async Task<ICustomer> CreateCustomerAsync()
        {
            try
            {
                if (!IsPrincipalValid(Principal))
                    throw new SecurityException(PrepareErrorMessage(Principal));

                if (Principal.IsEndUser())
                    return new Enduser(Principal.UserId(), Principal.EndUserListId(), DbContext);

                if (IsAdmin())
                    return new Admin(Principal.UserId(), Principal.CompanyId(), DbContext);

                var companies = await GetCompaniesWithAdminAccessAsync();
                return new User(Principal.UserId(), Principal.CompanyId(), companies,
                    GetScopes().ToArray(), DbContext);
            }
            catch (Exception)
            {
                throw new SecurityException();
            }
        }

        public static bool IsPrincipalValid(ClaimsPrincipal principal)
        {
            return principal.UserId() >= 0 && principal.CompanyId() >= 0;
        }

        private static string PrepareErrorMessage(ClaimsPrincipal principal)
        {
            var claim = principal.Claims.FirstOrDefault(c => c.Type == "UserId");
            var userId = claim == null ? "null" : Convert.ToInt32(claim.Value).ToString();
            claim = principal.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            var companyId = claim == null ? "null" : Convert.ToInt32(claim.Value).ToString();

            return $"Invalid principal. UserId = {userId}, CompanyId = {companyId}";
        }

        private bool IsAdmin()
        {
            return Principal.IsInRole("SYSTEM_ADMINISTRATE") ||
                   Principal.IsInRole("SYSTEM_PROJECT_ADMINISTRATE");
        }

        private async Task<int[]> GetCompaniesWithAdminAccessAsync()
        {
            var companies = new List<int>();
            var companiesWithAdminAccess =
                (await AccountLoader.GetAllCompaniesAsync(CompanyPermissionType.AdministratorRole))
                .Select(c => c.CompanyId);
            companies.AddRange(companiesWithAdminAccess);

            if (IsCompanyAdmin())
                companies.Add(Principal.CompanyId());

            return companies.Distinct().ToArray();
        }

        private bool IsCompanyAdmin()
        {
            return Principal.IsInRole("SYSTEM_COMPANY_ADMINISTRATE") ||
                   Principal.IsInRole("SYSTEM_COMPANY_PROJECT_ADMINISTRATE");
        }

        private IEnumerable<string> GetScopes()
        {
            if (Principal.HasClaim("scope", "api.surveyrights"))
                yield return "api.surveyrights";
            if (Principal.HasClaim("scope", "api.cdl.read"))
                yield return "api.cdl.read";
        }
    }
}