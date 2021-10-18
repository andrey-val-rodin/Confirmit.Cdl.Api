//==== DO NOT MODIFY THIS FILE ====
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.Common;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Authentication.Support;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    //DO NOT DELETE
    public partial class ScopeTests : IntegrationTestBase<ScopeFixture>, IClassFixture<ScopeFixture>
    {

        public ScopeTests(ScopeFixture fixture, ITestOutputHelper outputHelper) : base(outputHelper, fixture)
        {
        }

        //These tests will validate that the introspection middleware works as intended for this Api
        [Theory]
        [InlineData(IdpConstants.AccessTokenType.Jwt)]
        [InlineData(IdpConstants.AccessTokenType.Reference)]
        public async Task ValidScope_Ok(IdpConstants.AccessTokenType tokenType)
        {
            using var scope = Fixture.CreateScope();

            var testClient = scope.ServiceProvider.GetRequiredService<IConfirmitIntegrationTestsClient>();
            var confirmitTokenService = scope.ServiceProvider.GetRequiredService<IConfirmitTokenService>();
            var apiClient = scope.ServiceProvider.GetRequiredService<IHealthz>();

            var token = await GetToken(testClient, IntrospectionScope, tokenType);
            Assert.False(string.IsNullOrEmpty(token), "Could not get access token. Please check setup of the introspection scope for your application in appsettings.json and the Identity database");

            confirmitTokenService.SetToken(token);
            var scopeResponse = await apiClient.GetScope();
            Assert.True(HttpStatusCode.OK == scopeResponse.StatusCode, $"Expected status code: '{HttpStatusCode.OK}', but got '{scopeResponse.StatusCode}'. Please register the introspection scope for your application in the Identity database");
        }

        [Theory]
        [InlineData(IdpConstants.AccessTokenType.Jwt)]
        [InlineData(IdpConstants.AccessTokenType.Reference)]
        public async Task InvalidScope_Unauthorized(IdpConstants.AccessTokenType tokenType)
        {
            using var scope = Fixture.CreateScope();

            var testClient = scope.ServiceProvider.GetRequiredService<IConfirmitIntegrationTestsClient>();
            var confirmitTokenService = scope.ServiceProvider.GetRequiredService<IConfirmitTokenService>();
            var apiClient = scope.ServiceProvider.GetRequiredService<IHealthz>();

            var token = await GetToken(testClient, "dummy-scope-do-not-change", tokenType);
            confirmitTokenService.SetToken(token);
            var scopeResponse = await apiClient.GetScope();
            Assert.True(HttpStatusCode.Unauthorized == scopeResponse.StatusCode, $"Expected status code: '{HttpStatusCode.Unauthorized}', but got '{scopeResponse.StatusCode}'.");
        }

        private static async Task<string> GetToken(IConfirmitIntegrationTestsClient testClient, string introspectionScope, IdpConstants.AccessTokenType tokenType)
        {
            try
            {
                return await testClient.LoginUserAsync("administrator", "administrator", scope: introspectionScope, tokenType: tokenType);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}