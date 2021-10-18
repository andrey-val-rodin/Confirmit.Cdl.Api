using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Company = Confirmit.Cdl.Api.Accounts.Company;
using Enduser = Confirmit.Cdl.Api.Accounts.Enduser;
using EnduserCompany = Confirmit.Cdl.Api.Accounts.EnduserCompany;
using EnduserList = Confirmit.Cdl.Api.Accounts.EnduserList;
using User = Confirmit.Cdl.Api.Accounts.User;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class RefreshUserFixture : BaseFixture
    {
        private readonly SharedFixture _sharedFixture;

        public User User { get; private set; }
        public Company Company { get; private set; }
        public Enduser Enduser { get; private set; }
        public EnduserList EnduserList { get; private set; }
        public EnduserCompany EnduserCompany { get; private set; }

        public RefreshUserFixture(SharedFixture sharedFixture)
        {
            _sharedFixture = sharedFixture;
        }

        protected override void AddLocalServices(IServiceCollection services)
        {
            var uri = GetServiceUri();
            services.AddConfirmitClient<ICdl>(uri);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            using var scope = CreateScope();
            await _sharedFixture.UseAdminAsync(scope);

            await CreateEnduserAsync();
            await CreateUserAsync();
        }

        public override async Task DisposeAsync()
        {
            using var scope = CreateScope();
            await _sharedFixture.UseAdminAsync(scope);

            var dbWriter = new TestDbWriter();
            await dbWriter.DeleteCompanyAsync(Company.CompanyId);
            await dbWriter.DeleteEnduserCompanyAsync(EnduserCompany.Id, EnduserList.Id);
            await dbWriter.DeleteEnduserListAsync(EnduserList.Id);

            await base.DisposeAsync();
        }

        private async Task CreateUserAsync()
        {
            var realCompany = await Framework.Company.GetOrCreateAsync(_sharedFixture, "TestCompany3");
            var realUser = await Framework.User.GetOrCreateAsync(_sharedFixture, realCompany,
                "CDL_REFRESH_USER_TESTS", "Bob", "Abbey");

            User = new User
            {
                UserId = realUser.Id,
                UserName = realUser.Name,
                FirstName = realUser.FirstName,
                LastName = realUser.LastName,
                CompanyId = realUser.CompanyId,
                CompanyName = realUser.Company.Name
            };

            Company = new Company
            {
                CompanyId = realCompany.Id,
                Name = realCompany.Name
            };
        }

        private async Task CreateEnduserAsync()
        {
            var realEnduserList = await Framework.EnduserList.GetOrCreateAsync(
                _sharedFixture, "EnduserList_CDL_REFRESH_USER_TESTS");
            var realEnduserCompany = await Framework.EnduserCompany.GetOrCreateAsync(
                _sharedFixture, realEnduserList, "Company for EnduserList_CDL_REFRESH_USER_TESTS");
            var realEnduser = await Framework.Enduser.GetOrCreateAsync(
                _sharedFixture, realEnduserList, realEnduserCompany,
                "CDL_REFRESH_USER_TESTS", "Anna", "White", "anna.white@dummy.com");

            Enduser = new Enduser
            {
                Id = realEnduser.Id,
                Name = realEnduser.Name,
                FirstName = realEnduser.FirstName,
                LastName = realEnduser.LastName,
                Email = realEnduser.Email,
                ListId = realEnduser.ListId,
                CompanyId = realEnduser.CompanyId
            };

            EnduserList = new EnduserList
            {
                Id = realEnduserList.Id,
                Name = realEnduserList.Name
            };

            EnduserCompany = new EnduserCompany
            {
                Id = realEnduserCompany.Id,
                Name = realEnduserCompany.Name
            };
        }
    }
}
