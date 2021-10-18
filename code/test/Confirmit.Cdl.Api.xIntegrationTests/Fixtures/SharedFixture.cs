using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using Confirmit.NetCore.Common;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class SharedFixture : BaseFixture
    {
        private const string IntrospectionScope = "users endusers metadata cdl";
        private const string Postfix = CdlServiceClient.Postfix;
        private const string Prefix = CdlServiceClient.Prefix;

        private string _adminAccessToken;
        private string _prosUserAccessToken;
        private string _companyAdminAccessToken;
        private string _normalUserAccessToken;
        private string _normalUser2AccessToken;
        private string _enduserAccessToken;

        public User Admin;
        public User ProsUser;
        public User CompanyAdmin;
        public User NormalUser;
        public User NormalUser2;

        public Company TestCompany;
        public Company TestCompany2;

        public EnduserList EnduserList;
        public EnduserList EnduserList2;
        public Enduser Enduser;
        public Enduser Enduser2;
        public Enduser Enduser3;
        public Enduser Enduser4;
        public Enduser Enduser5;
        public Enduser InactiveEnduser;

        public Hub Hub1;
        public Hub Hub2;

        public Survey Survey1;
        public Survey Survey2;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // Remove old documents if previous test session failed
            await DeleteAllTestDocumentsAsync();

            await CreateAccountsAsync();
            await CreateHubsAsync();
            await CreateSurveysAsync();
        }

        public override async Task DisposeAsync()
        {
            await DeleteAllTestDocumentsAsync();
            await base.DisposeAsync();
        }

        private async Task CreateAccountsAsync()
        {
            var confirmitCompany = await Company.GetOrCreateAsync(this, "Confirmit");
            Admin = await User.GetOrCreateAsync(this, confirmitCompany,
                "administrator", "", "");

            TestCompany = await Company.GetOrCreateAsync(this, "TestCompany");
            ProsUser = await User.GetOrCreateAsync(this, TestCompany,
                "prosuser" + Postfix, "Pros", "User", "SYSTEM_PROJECT_ADMINISTRATE");
            CompanyAdmin = await User.GetOrCreateAsync(this, TestCompany,
                "companyadmin" + Postfix, "Company", "Admin", "SYSTEM_COMPANY_ADMINISTRATE");
            NormalUser = await User.GetOrCreateAsync(this, TestCompany,
                "normaluser" + Postfix, "Normal", "User");

            TestCompany2 = await Company.GetOrCreateAsync(this, "TestCompany2");
            NormalUser2 = await User.GetOrCreateAsync(this, TestCompany2,
                "normaluser2" + Postfix, "Normal", "User2");

            EnduserList = await EnduserList.GetOrCreateAsync(
                this, "EnduserList1" + Postfix);
            var enduserCompany = await EnduserCompany.GetOrCreateAsync(
                this, EnduserList, "Company for " + EnduserList.Name);
            Enduser = await Enduser.GetOrCreateAsync(
                this, EnduserList, enduserCompany,
                "Enduser", "Enduser", "1", "enduser@dummy.com");
            InactiveEnduser = await Enduser.GetOrCreateAsync(
                this, EnduserList, enduserCompany,
                "InactiveEnduser", "Enduser", "Inactive", "enduser@dummy.com");
            await new TestDbWriter().SetEnduserInactiveAsync(InactiveEnduser.Id);
            Enduser2 = await Enduser.GetOrCreateAsync(
                this, EnduserList, enduserCompany,
                "Enduser2", "Enduser", "2", "enduser2@dummy.com");

            EnduserList2 = await EnduserList.GetOrCreateAsync(this, "EnduserList2" + Postfix);
            var enduserCompany2 = await EnduserCompany.GetOrCreateAsync(
                this, EnduserList2, "Company for " + EnduserList2.Name);
            Enduser3 = await Enduser.GetOrCreateAsync(
                this, EnduserList2, enduserCompany2,
                "Enduser3", "Enduser", "3", "enduser3@dummy.com");
            Enduser4 = await Enduser.GetOrCreateAsync(
                this, EnduserList2, enduserCompany2,
                "Enduser4", "Enduser", "4", "enduser4@dummy.com");
            Enduser5 = await Enduser.GetOrCreateAsync(
                this, EnduserList2, enduserCompany2,
                "Enduser5", "Enduser", "5", "enduser5@dummy.com");
        }

        private async Task CreateHubsAsync()
        {
            Hub1 = await Hub.GetOrCreateAsync(this, $"{Prefix}Hub1");
            Hub2 = await Hub.GetOrCreateAsync(this, $"{Prefix}Hub2");
        }

        private async Task CreateSurveysAsync()
        {
            Survey1 = await Survey.GetOrCreateAsync(this, $"{Prefix}Survey1");
            Survey2 = await Survey.GetOrCreateAsync(this, $"{Prefix}Survey2");
        }

        private static async Task DeleteAllTestDocumentsAsync()
        {
            await new TestDbWriter().DeleteAllTestDocumentsAsync(Prefix);
        }

        protected override void AddLocalServices(IServiceCollection services)
        {
            var uri = GetServiceUri();
            services.AddConfirmitClient<ICdl>(uri);
            services.AddConfirmitClient<IEndusers>("endusers");
            services.AddConfirmitClient<IUsers>("users");
            services.AddConfirmitClient<IMetadata>("metaData");
            services.AddConfirmitClient<ISmartHub>("smartHub");
        }

        public async Task UseAdminAsync(IServiceScope scope)
        {
            if (string.IsNullOrEmpty(_adminAccessToken))
            {
                var testClient = scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginUserAsync(
                    "administrator", "administrator",
                    scope: IntrospectionScope);

                Assert.False(string.IsNullOrEmpty(token));
                _adminAccessToken = token;
            }

            scope.GetService<IConfirmitTokenService>().SetToken(_adminAccessToken);
        }

        public async Task UseProsUserAsync(IServiceScope scope)
        {
            if (string.IsNullOrEmpty(_prosUserAccessToken))
            {
                var testClient = scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginUserAsync(
                    ProsUser.Name, "password",
                    scope: IntrospectionScope);

                Assert.False(string.IsNullOrEmpty(token));
                _prosUserAccessToken = token;
            }

            scope.GetService<IConfirmitTokenService>().SetToken(_prosUserAccessToken);
        }

        public async Task UseCompanyAdminAsync(IServiceScope scope)
        {
            if (string.IsNullOrEmpty(_companyAdminAccessToken))
            {
                var testClient = scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginUserAsync(
                    CompanyAdmin.Name, "password",
                    scope: IntrospectionScope);

                Assert.False(string.IsNullOrEmpty(token));
                _companyAdminAccessToken = token;
            }

            scope.GetService<IConfirmitTokenService>().SetToken(_companyAdminAccessToken);
        }

        public async Task UseNormalUserAsync(IServiceScope scope)
        {
            if (string.IsNullOrEmpty(_normalUserAccessToken))
            {
                var testClient = scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginUserAsync(
                    NormalUser.Name, "password",
                    scope: IntrospectionScope);

                Assert.False(string.IsNullOrEmpty(token));
                _normalUserAccessToken = token;
            }

            scope.GetService<IConfirmitTokenService>().SetToken(_normalUserAccessToken);
        }

        public async Task UseNormalUser2Async(IServiceScope scope)
        {
            if (string.IsNullOrEmpty(_normalUser2AccessToken))
            {
                var testClient = scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginUserAsync(
                    NormalUser2.Name, "password",
                    scope: IntrospectionScope);

                Assert.False(string.IsNullOrEmpty(token));
                _normalUser2AccessToken = token;
            }

            scope.GetService<IConfirmitTokenService>().SetToken(_normalUser2AccessToken);
        }

        public async Task UseEnduserAsync(IServiceScope scope)
        {
            if (string.IsNullOrEmpty(_enduserAccessToken))
            {
                var testClient = scope.GetService<IConfirmitIntegrationTestsClient>();
                var token = await testClient.LoginEndUserAsync(
                    Enduser.Name, "password", Enduser.ListId,
                    scope: IntrospectionScope);

                Assert.False(string.IsNullOrEmpty(token));
                _enduserAccessToken = token;
            }

            scope.GetService<IConfirmitTokenService>().SetToken(_enduserAccessToken);
        }

        public void UseUnauthorizedUser(IServiceScope scope)
        {
            scope.GetService<IConfirmitTokenService>().SetToken(null);
        }
    }
}