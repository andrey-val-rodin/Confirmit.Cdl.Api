using Confirmit.Cdl.Api.Accounts.Clients;
using Confirmit.Cdl.Api.Tools;
using Confirmit.NetCore.Identity.Sdk.Clients;
using Confirmit.NetCore.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Accounts
{
    /// <summary>
    /// Loads accounts from external REST-services
    /// </summary>
    public sealed class AccountLoader : IAccountLoader
    {
        private const string UsersScope = "users";
        private const string EndusersScope = "endusers";
        private const string CompaniesScope = "metadata";
        private const string EndusersTrustedClaim = "api.endusers.read";

        private readonly ILogger<AccountLoader> _logger;
        private readonly IConfirmitTokenClient _tokenClient;

        private readonly IUsers _users;
        private readonly IEndusers _endusers;
        private readonly IMetadata _companies;

        public AccountLoader(
            ILogger<AccountLoader> logger,
            IConfirmitTokenClient tokenClient,
            IMetadata metadata,
            IUsers users,
            IEndusers endusers)
        {
            _logger = logger;
            _tokenClient = tokenClient;
            _companies = metadata;
            _users = users;
            _endusers = endusers;
        }

        public async Task<User> GetUserAsync(int userId)
        {
            var response = await InvokeAsync(UsersScope,
                    token => _users.GetUserAsync(userId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve user by id. Users service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var user = await response.Content.ReadAsAsync<User>().ConfigureAwait(false);
            return user;
        }

        public async Task<User> GetUserAsync(string userKey)
        {
            var response = await InvokeAsync(UsersScope,
                    token => _users.GetUserAsync(userKey, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve user by user key. Users service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var users = await response.Content.ReadAsAsync<Users>().ConfigureAwait(false);
            if (users?.Items == null || users.Items.Length == 0)
            {
                _logger.Warn(
                    "Unable to retrieve user by user key. Users service responds with null or empty user list");
                return null;
            }

            return users.Items[0];
        }

        public async Task<Company> GetCompanyAsync(int companyId)
        {
            var response = await InvokeAsync(CompaniesScope,
                    token => _companies.GetCompanyAsync(companyId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve company by Id = {companyId}. Metadata service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var company = await response.Content.ReadAsAsync<Clients.Company>().ConfigureAwait(false);
            return new Company { CompanyId = company.Id, Name = company.Name };
        }

        /// <remarks>
        /// Maximal number of companies returned by Metadata REST-service is 101
        /// This method receives 100 companies in every individual request and sends requests
        /// until response contains less than 100 items
        /// </remarks>
        public async Task<Company[]> GetAllCompaniesAsync(CompanyPermissionType permission)
        {
            var ret = new List<Company>();
            const int take = 100;
            int skip = 0;
            while (true)
            {
                var skip1 = skip;
                var response = await InvokeAsync(CompaniesScope,
                        token => _companies.GetCompaniesAsync(skip1, take, (int) permission, token))
                    .ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.Warn(
                        $"Unable to retrieve all companies, permission = {permission}. Metadata service responds with status code {response.StatusCode.ToString()}");
                    return new Company[0];
                }

                var companies = await response.Content.ReadAsAsync<Company[]>().ConfigureAwait(false);
                ret.AddRange(companies);
                if (companies.Length < take)
                    return ret.ToArray();

                skip += take;
            }
        }

        public async Task<User[]> GetUsersInCompanyAsync(int companyId)
        {
            var response = await InvokeAsync(UsersScope,
                    token => _users.GetUsersAsync(companyId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve users in company {companyId}. Users service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var users = await response.Content.ReadAsAsync<Users>().ConfigureAwait(false);
            if (users?.Items == null)
            {
                _logger.Warn(
                    $"Unable to retrieve users in company {companyId}. Users service responds with null user list");
                return null;
            }

            return users.Items;
        }

        public async Task<Enduser> GetEnduserAsync(int enduserId, bool useTrustedClaim = false)
        {
            var scope = EndusersScope;
            if (useTrustedClaim)
                scope = scope + " " + EndusersTrustedClaim;
            var response = await InvokeAsync(scope,
                    token => _endusers.GetEnduserAsync(enduserId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve enduser by Id = {enduserId}. Endusers service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var enduser = await response.Content.ReadAsAsync<Enduser>().ConfigureAwait(false);
            return enduser;
        }

        public async Task<EnduserList> GetEnduserListAsync(int listId)
        {
            var response = await InvokeAsync(EndusersScope,
                    token => _endusers.GetEnduserListAsync(listId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve enduser list by Id = {listId}. Endusers service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var enduserList = await response.Content.ReadAsAsync<EnduserList>().ConfigureAwait(false);
            return enduserList;
        }

        public async Task<EnduserCompany> GetEnduserCompanyAsync(int companyId, bool useTrustedClaim = false)
        {
            var scope = EndusersScope;
            if (useTrustedClaim)
                scope = scope + " " + EndusersTrustedClaim;
            var response = await InvokeAsync(scope,
                    token => _endusers.GetEnduserCompanyAsync(companyId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve enduser company by Id = {companyId}. Endusers service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var company = await response.Content.ReadAsAsync<EnduserCompany>().ConfigureAwait(false);
            return company;
        }

        public async Task<EnduserCompany[]> GetEnduserListCompaniesAsync(int listId)
        {
            var response = await InvokeAsync(EndusersScope,
                    token => _endusers.GetEnduserListCompaniesAsync(listId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve enduser list companies for enduser list{listId}. Endusers service responds with status code {response.StatusCode.ToString()}");
                return null;
            }

            var companies = await response.Content.ReadAsAsync<EnduserListCompanies>().ConfigureAwait(false);
            if (companies?.Items == null)
            {
                _logger.Warn(
                    $"Unable to retrieve companies in enduser list {listId}. Endusers service responds with null company list");
                return null;
            }

            return companies.Items;
        }

        public async Task<EnduserList[]> GetManyEnduserListsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                throw new ArgumentNullException(nameof(ids));

            var input = ids.Distinct().ToList();
            var enduserLists = new List<EnduserList>();
            foreach (var id in input)
            {
                var enduserList = await GetEnduserListAsync(id).ConfigureAwait(false);
                if (enduserList == null)
                    return new EnduserList[0];

                enduserLists.Add(enduserList);
            }

            return enduserLists.ToArray();
        }

        public async Task<Enduser[]> GetEndusersInListAsync(int listId)
        {
            // Endusers service returns empty array of endusers when enduser list does not exist,
            // so we have to retrieve enduser list at first to see if the specified listId is incorrect or
            // there is no permissions to read it
            if (await GetEnduserListAsync(listId).ConfigureAwait(false) == null)
                return null;

            var response = await InvokeAsync(EndusersScope,
                    token => _endusers.GetEndusersInListAsync(listId, token))
                .ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.Warn(
                    $"Unable to retrieve endusers in enduser list {listId}. Endusers service responds with status code {response.StatusCode.ToString()}");
                return new Enduser[0];
            }

            var endusers = await response.Content.ReadAsAsync<Endusers>().ConfigureAwait(false);
            if (endusers?.Items == null)
            {
                _logger.Warn(
                    $"Unable to retrieve endusers in enduser list {listId}. Endusers service responds with null user list");
                return null;
            }

            return endusers.Items.Where(eu => eu.IsActive).ToArray();
        }

        private async Task<T> InvokeAsync<T>(string scopes, Func<string, Task<T>> function,
            CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Invoking Service Client.");

            var token = await _tokenClient.GetAccessTokenAsync(scopes, cancellationToken)
                .ConfigureAwait(false);
            _logger.LogTrace("New token: " + token);

            var result = await function.Invoke("Bearer " + token).ConfigureAwait(false);

            _logger.LogTrace("Done invoking Service Client.");
            return result;
        }
    }
}