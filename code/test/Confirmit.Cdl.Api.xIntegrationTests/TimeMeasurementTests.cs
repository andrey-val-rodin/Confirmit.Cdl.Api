using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class TimeMeasurementTests : TestBase
    {
        public TimeMeasurementTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
            Output = outputHelper;
        }

        private ITestOutputHelper Output { get; }

        /// <summary>
        /// See analogical test in Cml.Storage.Service
        /// </summary>
        [Fact(Skip = "This test is for measurements only. Remove Skip to measure")]
        public async Task Measure()
        {
            // Create Http client
            var client = new HttpClient();

            // Get base url
            var uri = BaseFixture.GetServiceUri().ToString();

            // Authorize as normal user
            await SetAuthorizationHeaderAsync(client);

            int iterationNumber = 50;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (iterationNumber-- > 0)
            {
                // Create document and read response
                var response = await client.SendAsync(CreateDocumentPostRequest(uri));
                var document = JsonConvert.DeserializeObject<DocumentDto>(
                    await response.Content.ReadAsStringAsync());

                // Get document
                await client.GetAsync(uri + $"/documents/{document.Id}");

                // Update document
                await client.SendAsync(CreateDocumentPatchRequest(uri, document.Id));

                // Create few revisions
                await client.SendAsync(CreateRevisionPostRequest(uri, document.Id));
                await client.SendAsync(CreateRevisionPostRequest(uri, document.Id));

                // Get list of documents
                await client.GetAsync(uri + "/documents");

                // Get list of published revisions
                await client.GetAsync(uri + "/documents/revisions/published");

                // Get commits
                await client.GetAsync(uri + $"/documents/{document.Id}/commits");

                // Delete document
                await client.DeleteAsync(uri + $"/documents/{document.Id}");

                // Physically delete document
                await client.DeleteAsync(uri + $"/documents/{document.Id}/deleted");
            }
            stopWatch.Stop();

            // Get the elapsed time as a TimeSpan value.
            var timespan = stopWatch.Elapsed;
            Output.WriteLine($"Time elapsed: {timespan}");
        }

        #region Helpers

        private static HttpRequestMessage CreateDocumentPostRequest(string baseUri)
        {
            return new HttpRequestMessage(HttpMethod.Post, baseUri + "/documents")
            {
                Content = CreatePayloadForDocumentPost()
            };
        }

        private static HttpRequestMessage CreateDocumentPatchRequest(string baseUri, long documentId)
        {
            return new HttpRequestMessage(new HttpMethod("PATCH"), baseUri + $"/documents/{documentId}")
            {
                Content = CreatePayloadForDocumentPatch()
            };
        }

        private static HttpRequestMessage CreateRevisionPostRequest(string baseUri, long documentId)
        {
            return new HttpRequestMessage(HttpMethod.Post, baseUri + $"/documents/{documentId}/revisions")
            {
                Content = CreatePayloadForRevisionPost()
            };
        }

        private static StreamContent CreatePayloadForDocumentPost()
        {
            return CreateContent(new DocumentToCreateDto
            {
                Name = Prefix + "_measure",
                SourceCode = @"
title ""My document""
page #Dashboard {
  label: ""Dashboard""
  widget markdown {
    size: small
    markdown: ""This is Studio!!!""
  }
"
            });
        }

        private static StreamContent CreatePayloadForDocumentPatch()
        {
            return CreateContent(new DocumentPatchDto
            {
                SourceCode = @"
title ""My document""

config report {
  linkedSurveyId: ""p1027835""
}

page #Dashboard {
  label: ""Dashboard""
  widget markdown {
    size: small
    markdown: ""This is Studio!!!""
  }
"
            });
        }

        private static StreamContent CreatePayloadForRevisionPost()
        {
            return CreateContent(new RevisionToCreateDto
            {
                Name = Prefix + "_measure"
            });
        }

        private async Task SetAuthorizationHeaderAsync(HttpClient client)
        {
            var testClient = Scope.GetService<IConfirmitIntegrationTestsClient>();
            var token = await testClient.LoginUserAsync(
                NormalUser.Name, "password",
                scope: "cdl");
            Assert.False(string.IsNullOrEmpty(token));

            client.SetBearerToken(token);
        }

        private static StreamContent CreateContent<T>(T payload)
        {
            var stream = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(stream, payload);
            stream.Seek(0, 0);
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return content;
        }

        #endregion
    }
}
