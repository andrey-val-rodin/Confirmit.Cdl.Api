using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public class Enduser
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        public string Email { get; set; }
        public EnduserList List { get; set; }
        public EnduserCompany Company { get; set; }
        public string Password { get; set; }

        private Enduser()
        {
        }

        public static async Task<Enduser> GetOrCreateAsync(
            SharedFixture fixture,
            EnduserList list, EnduserCompany company,
            string name, string firstName, string lastName, string email)
        {
            using var scope = fixture.CreateScope();

            await fixture.UseAdminAsync(scope);
            var service = scope.GetService<IEndusers>();
            return await FindAsync(service, list, company, name, firstName, lastName, email) ??
                   await CreateAsync(service, list, company, name, firstName, lastName, email);
        }

        private static async Task<Enduser> FindAsync(
            IEndusers service,
            EnduserList enduserList, EnduserCompany company,
            string name, string firstName, string lastName, string email)
        {
            InternalUsers items;
            try
            {
                items = await service.GetEndusersAsync(enduserList.Id);
            }
            catch (Exception)
            {
                items = null;
            }

            var users = items?.Items;
            var user = users?.SingleOrDefault(u => u.Name.ToString() == name);
            if (user == null)
                return null;

            Assert.True(IsValid(user, company.Id, firstName, lastName, email),
                $"Something goes wrong. Remove enduser list {enduserList.Name} manually");

            user.List = enduserList;
            user.Company = company;

            return user;
        }

        private static bool IsValid(Enduser user, int companyId,
            string firstName, string lastName, string email)
        {
            return
                user.CompanyId == companyId &&
                user.FirstName == firstName &&
                user.LastName == lastName &&
                user.Email == email;
        }

        private static async Task<Enduser> CreateAsync(
            IEndusers service,
            EnduserList enduserList, EnduserCompany company,
            string name, string firstName, string lastName, string email)
        {
            var response = await service.CreateEnduserAsync(new Enduser
            {
                ListId = enduserList.Id,
                CompanyId = company.Id,
                Name = name,
                Password = "password",
                FirstName = firstName,
                LastName = lastName,
                Email = email
            });
            Assert.True(response.StatusCode == HttpStatusCode.Created, "Unable to create enduser");

            return await FindAsync(service, enduserList, company, name, firstName, lastName, email);
        }

        public class InternalUsers
        {
            [UsedImplicitly]
            public Enduser[] Items { get; set; }
        }
    }
}
