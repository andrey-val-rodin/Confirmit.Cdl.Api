using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class ConcurrentTests : TestBase
    {
        public ConcurrentTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        private async Task DoOperationsAsync()
        {
            // Create document
            var document = await PostDocumentAsync();

            // Get document
            await GetDocumentAsync(document.Id);

            // Update document
            await PatchDocumentAsync(document.Id, new DocumentPatchDto { SourceCode = "New CDL" });

            // Create few revisions
            await PostRevisionAsync(document.Id);
            await PostRevisionAsync(document.Id);

            // Get list of documents
            await GetDocumentsAsync();

            // Get list of published revisions
            await GetPublishedRevisionsAsync();

            // Get commits
            await GetCommitsAsync(document.Id);

            // Delete document
            await DeleteDocumentAsync(document.Id);
        }

        [Fact]
        public async Task Document_ConcurrentTest()
        {
            const int threadCount = 10;

            await UseNormalUserAsync();

            var tasks = new Task[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Factory.StartNew(() => DoOperationsAsync().Wait());
            }
            Task.WaitAll(tasks);
        }
    }
}
