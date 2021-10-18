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
    public class User
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName;
        public Company Company { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        private User()
        {
        }

        public static async Task<User> GetOrCreateAsync(
            SharedFixture fixture,
            Company company, string name,
            string firstName = null, string lastName = null, string permission = null)
        {
            using var scope = fixture.CreateScope();

            await fixture.UseAdminAsync(scope);
            var service = scope.GetService<IUsers>();
            return await FindAsync(service, company, name) ??
                   await CreateAsync(service, company, name, firstName, lastName, permission);
        }

        private static async Task<User> FindAsync(IUsers service, Company company, string name)
        {
            InternalUsers items;
            try
            {
                items = await service.GetUsersAsync(company.Id);
            }
            catch (Exception)
            {
                items = null;
            }

            var users = items?.Items;
            var user = users?.SingleOrDefault(u => u.UserName.ToString() == name);
            if (user == null)
                return null;

            var result = Copy(user);
            result.Company = company;
            return result;
        }

        private static User Copy(NetCore.Accounts.Sdk.Models.User user)
        {
            return new User
            {
                Id = user.UserId,
                CompanyId = user.CompanyId,
                Name = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        private static async Task<User> CreateAsync(
            IUsers service,
            Company company, string name,
            string firstName, string lastName, string permission)
        {
            var response = await service.CreateUserAsync(new User
            {
                Name = name,
                Password = "password",
                CompanyId = company.Id,
                FirstName = firstName,
                LastName = lastName,
                Email = "testuser1@confirmit.com"
            });
            Assert.True(response.StatusCode == HttpStatusCode.Created, "Unable to create user");

            var user = await FindAsync(service, company, name);
            Assert.NotNull(user);

            if (!string.IsNullOrEmpty(permission))
                await service.SetPermission(user.Id, permission);

            return user;
        }

        public class InternalUsers
        {
            [UsedImplicitly]
            public NetCore.Accounts.Sdk.Models.User[] Items { get; set; }
        }
    }
}
