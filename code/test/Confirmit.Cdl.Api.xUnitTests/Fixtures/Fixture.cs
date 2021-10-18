using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Database.Model;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.NetCore.Identity.Sdk.Claims;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xUnitTests.Fixtures
{
    [UsedImplicitly]
    public class Fixture : IAsyncDisposable
    {
        private const int UserId = 123;
        private const int CompanyId = 345;
        private const int EnduserId = 567;
        private const int EnduserListId = 789;

        public ClaimsPrincipal AdminPrincipal { get; private set; }
        public ClaimsPrincipal ProsPrincipal { get; private set; }
        public ClaimsPrincipal CompanyAdminPrincipal { get; private set; }
        public ClaimsPrincipal ProsCompanyPrincipal { get; private set; }
        public ClaimsPrincipal UserPrincipal { get; private set; }
        public ClaimsPrincipal UserPrincipal2 { get; private set; }
        public ClaimsPrincipal UserPrincipalWithClaimApiSurveyRights { get; private set; }
        public ClaimsPrincipal UserPrincipalWithClaimApiCdlRead { get; private set; }
        public ClaimsPrincipal EnduserPrincipal { get; private set; }

        public CdlDbContext Context { get; private set; }

        public Fixture()
        {
            CreatePrincipals();
            InitializeDbContext();
        }

        private void CreatePrincipals()
        {
            AdminPrincipal = CreateUserPrincipal(1, 1, "SYSTEM_ADMINISTRATE");
            ProsPrincipal = CreateUserPrincipal(UserId, 1, "SYSTEM_PROJECT_ADMINISTRATE");
            CompanyAdminPrincipal = CreateUserPrincipal(UserId, 1, "SYSTEM_COMPANY_ADMINISTRATE");
            ProsCompanyPrincipal = CreateUserPrincipal(UserId, 1, "SYSTEM_COMPANY_PROJECT_ADMINISTRATE");
            UserPrincipal = CreateUserPrincipal(UserId, CompanyId);
            UserPrincipal2 = CreateUserPrincipal(999, 1);
            UserPrincipalWithClaimApiSurveyRights = CreateUserPrincipal(1001, 2, claim: "api.surveyrights");
            UserPrincipalWithClaimApiCdlRead = CreateUserPrincipal(1002, 2, claim: "api.cdl.read");
            EnduserPrincipal = CreateEnduserPrincipal(EnduserId, CompanyId, EnduserListId);
        }

        private static ClaimsPrincipal CreateUserPrincipal(int userId, int companyId,
            string role = null, string claim = null)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("UserId", userId.ToString()));
            identity.AddClaim(new Claim("CompanyId", companyId.ToString()));
            if (!string.IsNullOrEmpty(role))
                identity.AddClaim(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role));
            if (!string.IsNullOrEmpty(claim))
                identity.AddClaim(new Claim("scope", claim));

            return new ClaimsPrincipal(identity);
        }

        private static ClaimsPrincipal CreateEnduserPrincipal(int enduserId, int companyId, int enduserListId)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim("UserId", enduserId.ToString()));
            identity.AddClaim(new Claim("CompanyId", companyId.ToString()));
            identity.AddClaim(new Claim("EnduserListId", enduserListId.ToString()));
            identity.AddClaim(new Claim("sub", "eu"));

            return new ClaimsPrincipal(identity);
        }

        private void InitializeDbContext()
        {
            var options = new DbContextOptionsBuilder<CdlDbContext>()
                .UseInMemoryDatabase("cdl_tests")
                .Options;

            Context = new CdlDbContext(options);

            Context.Documents.Add(new Document { Id = 1, CompanyId = 1 });
            Context.Documents.Add(new Document { Id = 2, CompanyId = 1 });
            Context.Documents.Add(new Document { Id = 3, CompanyId = 2 });
            Context.Documents.Add(new Document { Id = 4, CompanyId = 3, Type = (byte) DocumentType.DataTemplate });
            Context.Documents.Add(new Document { Id = 5, CompanyId = 2, PublishedRevisionId = 1 });
            Context.Documents.Add(new Document { Id = 6, CompanyId = 1, Deleted = DateTime.UtcNow });

            Context.Revisions.Add(new Revision { Id = 1, DocumentId = 5 });

            AddUserPermissions();
            AddEnduserPermissions();

            Context.SaveChanges();
        }

        private void AddUserPermissions()
        {
            Context.UserPermissions.Add(new UserPermission
            {
                DocumentId = 2,
                UserId = UserId,
                Permission = (byte) Permission.Manage
            });
            Context.UserPermissions.Add(new UserPermission
            {
                DocumentId = 3,
                UserId = UserId,
                Permission = (byte) Permission.Manage
            });
            Context.UserPermissions.Add(new UserPermission
            {
                DocumentId = 6,
                UserId = UserId,
                Permission = (byte) Permission.View
            });
            Context.UserPermissions.Add(new UserPermission
            {
                DocumentId = 4,
                UserId = UserPrincipalWithClaimApiSurveyRights.UserId(),
                Permission = (byte) Permission.View
            });
            Context.UserPermissions.Add(new UserPermission
            {
                DocumentId = 1,
                UserId = UserPrincipalWithClaimApiCdlRead.UserId(),
                Permission = (byte) Permission.Manage
            });
            Context.UserPermissions.Add(new UserPermission
            {
                DocumentId = 2,
                UserId = UserPrincipalWithClaimApiCdlRead.UserId(),
                Permission = (byte) Permission.View
            });
        }

        private void AddEnduserPermissions()
        {
            Context.EnduserPermissions.Add(new EnduserPermission
            {
                DocumentId = 3,
                UserId = EnduserId,
                Permission = (byte) Permission.View
            });
            Context.EnduserPermissions.Add(new EnduserPermission
            {
                DocumentId = 5,
                UserId = EnduserId,
                Permission = (byte) Permission.View
            });
            Context.EnduserPermissions.Add(new EnduserPermission
            {
                DocumentId = 6,
                UserId = EnduserId,
                Permission = (byte) Permission.View
            });
        }

        public async ValueTask DisposeAsync()
        {
            if (Context != null)
                await Context.DisposeAsync();
        }
    }
}
