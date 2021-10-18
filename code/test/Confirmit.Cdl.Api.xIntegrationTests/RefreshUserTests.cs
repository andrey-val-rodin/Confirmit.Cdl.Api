using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Common;
using Confirmit.NetCore.Identity.Sdk.Claims;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Company = Confirmit.Cdl.Api.Accounts.Company;
using Enduser = Confirmit.Cdl.Api.Accounts.Enduser;
using EnduserCompany = Confirmit.Cdl.Api.Accounts.EnduserCompany;
using EnduserList = Confirmit.Cdl.Api.Accounts.EnduserList;
using User = Confirmit.Cdl.Api.Accounts.User;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class RefreshUserTests : TestBase, IClassFixture<RefreshUserFixture>
    {
        private readonly RefreshUserFixture _fixture;
        private string _realUserAccessToken;
        private string _realEnduserAccessToken;

        public RefreshUserTests(
            SharedFixture sharedFixture, RefreshUserFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region Refresh (direct DbRefresher calls)

        [Fact]
        public async Task Refresh_NewUser_DatabaseContainsNewUserAndCompany()
        {
            // Wipe DB
            await new TestDbWriter().DeleteCompanyAsync(_fixture.Company.CompanyId); // deletes users also

            // Create principal and refresh DB
            var principal = CreatePrincipal(_fixture.User);
            await new DbRefresher(null, principal).RefreshAsync();

            // DB contains valid data now
            AssertValidUserInLocalDb(_fixture.User);
            AssertValidCompanyInLocalDb(_fixture.Company);
        }

        [Fact]
        public async Task Refresh_UpdateUserLastName_DatabaseContainsUpdatedUser()
        {
            // Insert user's data into DB
            var principal = CreatePrincipal(_fixture.User);
            await new DbRefresher(null, principal).RefreshAsync();

            // Change last name
            var user = CreateUser(principal);
            user.LastName = "updated";
            principal = CreatePrincipal(user);
            await new DbRefresher(null, principal).RefreshAsync();

            // DB contains updated user
            AssertValidUserInLocalDb(CreateUser(principal));
        }

        [Fact]
        public async Task RefreshUser_DatabaseDoesNotContainCompany_Exception()
        {
            // Wipe DB
            await new TestDbWriter().DeleteCompanyAsync(_fixture.Company.CompanyId);

            var principal = CreatePrincipal(_fixture.User);

            // DbRefresher can't insert new user because local database doesn't contain company
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new DbRefresher(null, principal).RefreshUserAsync(_fixture.User));
        }

        [Fact]
        public async Task RefreshUser_DatabaseContainsCompany_DatabaseContainsNewUser()
        {
            // Wipe DB
            await new TestDbWriter().DeleteCompanyAsync(_fixture.Company.CompanyId); // deletes users also

            // Append company and refresh user
            await new QueryProvider().InsertCompanyAsync(_fixture.Company);
            var principal = CreatePrincipal(_fixture.User);
            await new DbRefresher(null, principal).RefreshAsync();

            // DB contains new user
            AssertValidUserInLocalDb(_fixture.User);
        }

        [Fact]
        public async Task RefreshCompany_DatabaseContainsNewCompany()
        {
            // Wipe DB
            await new TestDbWriter().DeleteCompanyAsync(_fixture.Company.CompanyId);

            // Refresh company
            var principal = CreatePrincipal(_fixture.User);
            await new DbRefresher(null, principal).RefreshCompanyAsync(_fixture.Company);

            // DB contains new company
            AssertValidCompanyInLocalDb(_fixture.Company);
        }

        [Fact]
        public async Task Refresh_UpdateCompanyName_DatabaseContainsUpdatedCompany()
        {
            // Insert user's data into DB
            var principal = CreatePrincipal(_fixture.User);
            await new DbRefresher(null, principal).RefreshAsync();

            // Change company name
            var user = CreateUser(principal);
            user.CompanyName = "updated";
            principal = CreatePrincipal(user);
            await new DbRefresher(null, principal).RefreshAsync();

            // DB contains updated company
            AssertValidCompanyInLocalDb(CreateCompany(principal));
        }

        [Fact]
        public async Task RefreshEnduser_DatabaseDoesNotContainEnduserList_Exception()
        {
            // Wipe DB
            await new TestDbWriter().DeleteEnduserListAsync(_fixture.EnduserList.Id); // deletes endusers also

            var principal = CreatePrincipal(_fixture.Enduser);

            // DbRefresher can't insert new enduser because local database doesn't contain enduser list
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                new DbRefresher(null, principal).RefreshEnduserAsync(_fixture.Enduser));
        }

        [Fact]
        public async Task RefreshEnduser_DatabaseContainsEnduserList_DatabaseContainsNewEnduser()
        {
            // Wipe DB
            await new TestDbWriter().DeleteEnduserListAsync(_fixture.EnduserList.Id); // deletes endusers also

            // Append enduser list and refresh enduser
            await new QueryProvider().InsertEnduserListAsync(_fixture.EnduserList);
            var principal = CreatePrincipal(_fixture.Enduser);
            await new DbRefresher(null, principal).RefreshEnduserAsync(_fixture.Enduser);

            // DB contains new enduser
            AssertValidEnduserInLocalDb(_fixture.Enduser);
        }

        [Fact]
        public async Task RefreshEnduserList_DatabaseContainsUpdatedEnduserList()
        {
            // Wipe DB
            await new TestDbWriter().DeleteEnduserListAsync(_fixture.EnduserList.Id);

            // Refresh enduser list
            var principal = CreatePrincipal(_fixture.Enduser);
            await new DbRefresher(null, principal).RefreshEnduserListAsync(_fixture.EnduserList);

            // Db contains valid enduser list now
            AssertValidEnduserListInLocalDb(_fixture.EnduserList);
        }

        [Fact]
        public async Task RefreshEnduserCompany_DatabaseContainsUpdatedEnduserCompany()
        {
            // Wipe DB
            await new TestDbWriter().DeleteEnduserCompanyAsync(_fixture.EnduserCompany.Id, _fixture.EnduserList.Id);

            // Refresh enduser company
            var principal = CreatePrincipal(_fixture.Enduser);
            await new DbRefresher(null, principal).RefreshEnduserCompanyAsync(_fixture.EnduserCompany);

            // Db contains valid enduser company now
            AssertValidEnduserCompanyInLocalDb(_fixture.EnduserCompany);
        }

        #endregion

        #region Service calls

        [Fact]
        public async Task User_GetDocuments_DatabaseContainsNewEnduser()
        {
            // Wipe DB
            await new TestDbWriter().DeleteCompanyAsync(_fixture.Company.CompanyId); // deletes users also

            await UseRealUserAsync();

            // The service refreshes current principal in background thread in each route.
            // We can use any other endpoint here
            await GetDocumentsAsync();

            AssertValidUserInLocalDb(_fixture.User);
            AssertValidCompanyInLocalDb(_fixture.Company);
        }

        [Fact]
        public async Task Enduser_GetAliases_DatabaseContainsNewEnduser()
        {
            // Wipe DB
            await new TestDbWriter().DeleteEnduserListAsync(_fixture.EnduserList.Id); // deletes endusers also

            await UseRealEnduserAsync();

            // The service refreshes current principal in background thread in each route.
            // We can use any other endpoint available to endusers here
            await GetAliasesAsync();

            // DB contains valid data now
            AssertValidEnduserInLocalDb(_fixture.Enduser);
            AssertValidEnduserListInLocalDb(_fixture.EnduserList);
            AssertValidEnduserCompanyInLocalDb(_fixture.EnduserCompany);
        }

        #endregion

        #region Helpers

        private async Task UseRealUserAsync()
        {
            if (string.IsNullOrEmpty(_realUserAccessToken))
            {
                var testClient = Scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginUserAsync(
                    _fixture.User.UserName, "password",
                    scope: "cdl");

                Assert.False(string.IsNullOrEmpty(token));
                _realUserAccessToken = token;
            }

            Scope.GetService<IConfirmitTokenService>().SetToken(_realUserAccessToken);
        }

        private async Task UseRealEnduserAsync()
        {
            if (string.IsNullOrEmpty(_realEnduserAccessToken))
            {
                var testClient = Scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginEndUserAsync(
                    _fixture.Enduser.Name, "password", _fixture.Enduser.ListId,
                    scope: "cdl");

                Assert.False(string.IsNullOrEmpty(token));
                _realEnduserAccessToken = token;
            }

            Scope.GetService<IConfirmitTokenService>().SetToken(_realEnduserAccessToken);
        }

        private static ClaimsPrincipal CreatePrincipal(User user)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", user.UserName),
                new Claim("CompanyId", user.CompanyId.ToString()),
                new Claim("CompanyName", user.CompanyName),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", user.FirstName),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", user.LastName)
            }));
        }

        private static ClaimsPrincipal CreatePrincipal(Enduser enduser)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("UserId", enduser.Id.ToString()),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", enduser.Name),
                new Claim("CompanyId", enduser.CompanyId.ToString()),
                new Claim("EndUserListId", enduser.ListId.ToString()),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname", enduser.FirstName),
                new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname", enduser.LastName)
            }));
        }

        private static User CreateUser(ClaimsPrincipal principal)
        {
            return new User
            {
                UserId = principal.UserId(),
                UserName = principal.UserName(),
                CompanyId = principal.CompanyId(),
                CompanyName = principal.CompanyName(),
                FirstName = principal.FirstName(),
                LastName = principal.LastName()
            };
        }

        private static Company CreateCompany(ClaimsPrincipal principal)
        {
            return new Company
            {
                CompanyId = principal.CompanyId(),
                Name = principal.CompanyName()
            };
        }

        private static void AssertValidUserInLocalDb(User user)
        {
            // The service updates users very quickly,
            // so we don't need to wait like in AssertValidEnduserInLocalDb
            var userInDb = new QueryProvider().GetUserAsync(user.UserId).Result;

            Assert.Equal(user.UserId, userInDb.UserId);
            Assert.Equal(user.CompanyId, userInDb.CompanyId);
            Assert.Equal(user.UserName, userInDb.UserName);
            Assert.Equal(user.FirstName, userInDb.FirstName);
            Assert.Equal(user.LastName, userInDb.LastName);
        }

        private static void AssertValidCompanyInLocalDb(Company company)
        {
            var companyInDb = new QueryProvider().GetCompanyAsync(company.CompanyId).Result;

            Assert.NotNull(companyInDb);
            Assert.Equal(company.CompanyId, companyInDb.CompanyId);
            Assert.Equal(company.Name, companyInDb.Name);
        }

        private static void AssertValidEnduserInLocalDb(Enduser enduser)
        {
            // Wait until the service refreshes enduser
            int attemptCount = 20;
            Enduser enduserInDb = null;
            while (attemptCount > 0)
            {
                enduserInDb = new QueryProvider().GetEnduserAsync(enduser.Id).Result;
                if (enduserInDb != null)
                    break;

                Thread.Sleep(300);
                attemptCount--;
            }

            Assert.NotNull(enduserInDb);
            Assert.Equal(enduser.Id, enduserInDb.Id);
            Assert.Equal(enduser.ListId, enduserInDb.ListId);
            Assert.Equal(enduser.Name, enduserInDb.Name);
            Assert.Equal(enduser.FirstName, enduserInDb.FirstName);
            Assert.Equal(enduser.LastName, enduserInDb.LastName);
            Assert.Equal(enduser.CompanyId, enduserInDb.CompanyId);
            Assert.Equal(enduser.Email, enduserInDb.Email);
        }

        private static void AssertValidEnduserListInLocalDb(EnduserList enduserList)
        {
            var enduserListInDb = new QueryProvider().GetEnduserListAsync(enduserList.Id).Result;

            Assert.NotNull(enduserListInDb);
            Assert.Equal(enduserList.Id, enduserListInDb.Id);
            Assert.Equal(enduserList.Name, enduserListInDb.Name);
        }

        private static void AssertValidEnduserCompanyInLocalDb(EnduserCompany company)
        {
            var companyInDb = new QueryProvider().GetEnduserCompanyAsync(company.Id).Result;

            Assert.NotNull(companyInDb);
            Assert.Equal(company.Id, companyInDb.Id);
            Assert.Equal(company.Name, companyInDb.Name);
        }

        #endregion
    }
}
