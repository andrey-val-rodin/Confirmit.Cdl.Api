using JetBrains.Annotations;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    public static class ResponseHandler
    {
        [AssertionMethod]
        public static async Task<T> HandleRequestAsync<T>(
            HttpResponseMessage response,
            HttpStatusCode expectedStatusCode,
            string expectedErrorMessage)
        {
            Assert.True(expectedStatusCode == response.StatusCode,
                $"Wrong status code. Expected: {expectedStatusCode} Actual: {response.StatusCode}");

            string content = null;
            if (expectedErrorMessage != null)
            {
                content = await response.Content.ReadAsStringAsync();
                Assert.True(content.Contains(expectedErrorMessage),
                    $"Response doesn't contain expected error message '{expectedErrorMessage}'");
            }

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                return default;

            if (string.IsNullOrEmpty(content))
                content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        [AssertionMethod]
        public static async Task<Stream> HandleRequestAsync(
            HttpResponseMessage response,
            HttpStatusCode expectedStatusCode)
        {
            Assert.True(expectedStatusCode == response.StatusCode,
                $"Wrong status code. Expected: {expectedStatusCode} Actual: {response.StatusCode}");

            Stream stream = null;
            try
            {
                stream = await response.Content.ReadAsStreamAsync();
            }
            catch (HttpRequestException e)
            {
                if (expectedStatusCode == HttpStatusCode.OK)
                    Assert.True(false, $"Unexpected exception '{e.Message}'");
                else
                    Assert.Equal("Response status code does not indicate success: 404 (Not Found).", e.Message);
            }

            return stream;
        }

        [AssertionMethod]
        public static async Task HandleRequestAsync(
            HttpResponseMessage response,
            HttpStatusCode expectedStatusCode,
            string expectedErrorMessage)
        {
            Assert.True(expectedStatusCode == response.StatusCode,
                $"Wrong status code. Expected: {expectedStatusCode} Actual: {response.StatusCode}");

            if (expectedErrorMessage != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.True(content.Contains(expectedErrorMessage),
                    $"Response doesn't contain expected error message '{expectedErrorMessage}'");
            }
        }
    }
}
