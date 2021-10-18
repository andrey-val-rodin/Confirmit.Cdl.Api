using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public class Survey
    {
        public string ProjectId { get; set; }
        public string Name { get; set; }

        public static async Task<Survey> GetOrCreateAsync(SharedFixture fixture, string name)
        {
            using var scope = fixture.CreateScope();

            // Create survey under NormalUser account
            await fixture.UseNormalUserAsync(scope);
            var service = scope.GetService<IMetadata>();
            return await FindAsync(service, name) ?? await CreateAsync(service, name);
        }

        private static async Task<Survey> FindAsync(IMetadata service, string name)
        {
            try
            {
                var projects = await service.GetProjectsAsync($"Name eq '{name}'");
                return projects?.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<Survey> CreateAsync(IMetadata service, string name)
        {
            // This request creates new project with name "New Project
            await service.CreateProjectAsync("Project");
            var survey = await FindAsync(service, "New Project");
            Assert.NotNull(survey);

            // Replace name
            await service.SetProjectName(survey.ProjectId, new JObject { ["Name"] = name });

            return await FindAsync(service, name);
        }
    }
}
