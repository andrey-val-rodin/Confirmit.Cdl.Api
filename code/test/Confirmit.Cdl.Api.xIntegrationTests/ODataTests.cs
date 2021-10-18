using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class ODataTests : TestBase, IClassFixture<ODataFixture>
    {
        private readonly ODataFixture _fixture;

        public ODataTests(SharedFixture sharedFixture, ODataFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region GetDocuments

        [Fact]
        public async Task GetDocuments_NonexistentProperty_BadRequest()
        {
            await UseNormalUserAsync();
            await GetDocumentsAsync(orderBy: "NonexistentProperty",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedErrorMessage:
                "Could not find a property named 'NonexistentProperty' on type 'Confirmit.Cdl.Api.ViewModel.DocumentShortDto'.");
        }

        [Fact]
        public async Task GetDocuments_NonexistentType_BadRequest()
        {
            await UseNormalUserAsync();
            await GetDocumentsAsync(filter: "Type ne 'NonexistentType'",
                expectedStatusCode: HttpStatusCode.BadRequest,
                expectedErrorMessage: "The string 'NonexistentType' is not a valid enumeration type constant.");
        }

        [Fact]
        public async Task GetDocuments_NegativeSkip_BadRequest()
        {
            await UseNormalUserAsync();
            await GetDocumentsAsync(skip: -1,
                expectedStatusCode: HttpStatusCode.BadRequest, expectedErrorMessage:
                "Invalid value '-1' for $skip query option found. The $skip query option requires a non-negative integer value.");
        }

        [Fact]
        public async Task GetDocuments_NegativeTop_BadRequest()
        {
            await UseNormalUserAsync();
            await GetDocumentsAsync(top: -1,
                expectedStatusCode: HttpStatusCode.BadRequest, expectedErrorMessage:
                "Invalid value '-1' for $top query option found. The $top query option requires a non-negative integer value.");
        }

        [Fact]
        public async Task GetDocuments_ZeroTop_PageIsEmpty()
        {
            await UseAdminAsync();
            var result = await GetDocumentsAsync(top: 0);

            Assert.Equal(0, result.ItemCount);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task GetDocuments_ZeroTop_ValidTotalCount()
        {
            await UseAdminAsync();
            var result = await GetDocumentsAsync(top: 0, filter: $"contains(Name, '{_fixture.Name}')");

            Assert.Equal(4, result.TotalCount);
        }

        [Fact]
        public async Task GetDocuments_Name_AllDocuments()
        {
            await UseAdminAsync();
            var result = await GetDocumentsAsync(filter: $"contains(Name, '{_fixture.Name}')");

            Assert.Equal(4, result.Items.Count);
            Assert.Contains(result.Items, d => d.Id == _fixture.Doc1.Id);
            Assert.Contains(result.Items, d => d.Id == _fixture.Doc2.Id);
            Assert.Contains(result.Items, d => d.Id == _fixture.Doc3.Id);
            Assert.Contains(result.Items, d => d.Id == _fixture.Doc4.Id);
        }

        [Fact]
        public async Task GetDocuments_Name_AvailableDocuments()
        {
            await UseNormalUserAsync();
            var result = await GetDocumentsAsync(filter: $"contains(Name, '{_fixture.Name}')");

            Assert.Equal(3, result.Items.Count);
            Assert.Contains(result.Items, d => d.Id == _fixture.Doc1.Id);
            Assert.Contains(result.Items, d => d.Id == _fixture.Doc2.Id);
            Assert.Contains(result.Items, d => d.Id == _fixture.Doc3.Id);
        }

        [Fact]
        public async Task GetDocuments_TopAndSkip_ValidPreviousPageLink()
        {
            await UseNormalUserAsync();
            var result = await GetDocumentsAsync(top: 2, skip: 1, orderBy: "Created asc");
            var prev = result.Links.PreviousPage;

            Assert.NotNull(prev);
            Assert.Contains("?$skip=0&$top=2&$orderby=Created%20asc", prev); // skip = 0
        }

        [Fact]
        public async Task GetDocuments_TopAndSkip_ValidNextPageLink()
        {
            await UseNormalUserAsync();
            var result = await GetDocumentsAsync(top: 1, skip: 1, orderBy: "Created asc");
            var next = result.Links.NextPage;

            Assert.NotNull(next);
            Assert.Contains("?$skip=2&$top=1&$orderby=Created%20asc", next); // skip = 2
        }

        [Fact]
        public async Task GetDocuments_TopAndSkip_ValidTotalCount()
        {
            await UseNormalUserAsync();
            var result = await GetDocumentsAsync(top: 1, skip: 1, filter: $"contains(Name, '{_fixture.Name}')");

            Assert.Single(result.Items);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetDocuments_ComplexCondition_ValidResult()
        {
            await UseNormalUserAsync();
            var n = _fixture.Name;
            var result = await GetDocumentsAsync(top: 20, skip: 0, orderBy: "Modified desc",
                filter:
                $"Type ne 'ProgramDashboard' and (contains(Name, '{n}') or contains(CompanyName, '{n}') or contains(CreatedByName, '{n}') or contains(ModifiedByName, '{n}'))");

            Assert.Equal(3, result.ItemCount);
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(3, result.Items.Count);
        }

        [Fact]
        public async Task GetDocuments_Enduser_Forbidden()
        {
            await UseEnduserAsync();
            await GetDocumentsAsync(filter: $"contains(Name, '{_fixture.Name}')",
                expectedStatusCode: HttpStatusCode.Forbidden);
        }

        #endregion

        #region GetAccessedDocuments

        [Fact]
        public async Task GetAccessedDocuments_OrderByAccessedAsc_ValidResult()
        {
            await UseAdminAsync();
            var result = await GetAccessedDocumentsAsync(orderBy: "Accessed asc");
            Assert.True(result.Items.Count >= 4);

            // Check order
            DateTime? prev = null;
            foreach (var document in result.Items)
            {
                Assert.True(prev == null || document.Accessed == null || prev <= document.Accessed);
                prev = document.Accessed;
            }
        }

        [Fact]
        public async Task GetAccessedDocuments_OrderByAccessedDesc_ValidResult()
        {
            await UseAdminAsync();
            var result = await GetAccessedDocumentsAsync(orderBy: "Accessed desc");
            Assert.True(result.Items.Count >= 4);

            // Check order
            DateTime? prev = null;
            foreach (var document in result.Items)
            {
                Assert.True(prev == null || document.Accessed == null || prev >= document.Accessed);
                prev = document.Accessed;
            }
        }

        [Fact]
        public async Task GetAccessedDocuments_OrderByAccessedThenByCreated_ValidResult()
        {
            await UseNormalUserAsync();
            var result = await GetAccessedDocumentsAsync(orderBy: "Accessed desc,Created desc");

            Assert.True(result.Items.Count >= 3, "Normal user has access to at least 3 documents");
            AssertOrderIsValid(result.Items);
        }

        [Fact]
        public async Task GetAccessedDocuments_AccessedIsNotNull_AvailableDocuments()
        {
            await UseNormalUserAsync();
            var result = await GetAccessedDocumentsAsync(filter: "Accessed ne null");

            Assert.True(result.Items.All(i => i.Accessed != null));
        }

        #endregion

        #region GetArchivedDocuments

        [Fact]
        public async Task GetArchivedDocuments_Admin_ValidResult()
        {
            await UseAdminAsync();
            var result = await GetArchivedDocumentsAsync(orderBy: "Deleted asc");

            Assert.Contains(result.Items, d => d.Id == _fixture.DeletedDoc.Id);
        }

        [Fact]
        public async Task GetArchivedDocuments_NormalUser_ValidResult()
        {
            await UseNormalUserAsync();
            var result = await GetArchivedDocumentsAsync(orderBy: "Deleted asc");

            // NormalUser has not access to _fixture.DeletedDoc
            Assert.DoesNotContain(result.Items, d => d.Id == _fixture.DeletedDoc.Id);
        }

        #endregion

        #region GetDocumentRevisions

        [Fact]
        public async Task GetDocumentRevisions_NormalUser_ValidResult()
        {
            await UseNormalUserAsync();
            var result = await GetRevisionsAsync(_fixture.Doc1.Id,
                filter: $"(Type eq 'ProgramDashboard' or Type eq 'DataFlow') and DocumentId eq {_fixture.Doc1.Id}");

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
        }

        #endregion

        #region GetPublishedRevisions

        [Fact]
        public async Task GetPublishedRevisions_Enduser_ValidResult()
        {
            await UseEnduserAsync();
            var result = await GetPublishedRevisionsAsync(top: 20, skip: 0, filter: $"Name eq '{_fixture.Doc1.Name}'");

            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal(_fixture.Doc1.Id, result.Items[0].DocumentId);
        }

        #endregion

        #region GetAccessedRevisions

        [Fact]
        public async Task GetAccessedRevisions_OrderByAccessedThenByCreated_ValidResult()
        {
            await UseEnduserAsync();
            var result = await GetAccessedRevisionsAsync(orderBy: "Accessed desc,Created desc");

            Assert.True(result.Items.Count >= 2, "Enduser has access to at least 2 revisions");
            AssertOrderIsValid(result.Items);
        }

        [Fact]
        public async Task GetAccessedRevisions_AccessedIsNotNull_AvailableDocuments()
        {
            await UseEnduserAsync();
            var result = await GetAccessedRevisionsAsync(filter: "Accessed ne null");

            Assert.True(result.Items.All(i => i.Accessed != null));
        }

        #endregion

        #region GetAliases

        [Fact]
        public async Task GetAliases_Admin_AllAliases()
        {
            await UseAdminAsync();
            var page = await GetAliasesAsync();

            Assert.True(page.TotalCount >= _fixture.AllAliases.Count);
        }

        [Fact]
        public async Task GetAliases_SearchByNamespace_AvailableAliases()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "Namespace eq 'ons_a'")).Items;

            Assert.Equal(_fixture.AllAliases.Count(a => a.Namespace == "ons_a"), aliases.Count);
        }

        [Fact]
        public async Task GetAliases_PartialSearchByNamespace_AvailableAliases()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')")).Items;

            Assert.Equal(_fixture.AllAliases.Count(a => a.Namespace.Contains("ons_")), aliases.Count);
        }

        [Fact]
        public async Task GetAliases_SearchByAlias_AvailableAliases()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_') and Alias eq 'green'")).Items;

            Assert.Equal(_fixture.AllAliases.Count(a => a.Alias == "green"), aliases.Count);
        }

        [Fact]
        public async Task GetAliases_PartialSearchByAlias_AvailableAliases()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_') and startswith(Alias, 'gr')"))
                .Items;

            Assert.Equal(_fixture.AllAliases.Count(a => a.Alias.Contains("gr")), aliases.Count);
        }

        [Fact]
        public async Task GetAliases_SearchByInvalidDocumentId_BadRequest()
        {
            await UseAdminAsync();
            await GetAliasesAsync(filter: "DocumentId eq 'INVALID'", expectedStatusCode: HttpStatusCode.BadRequest,
                expectedErrorMessage:
                "A binary operator with incompatible types was detected. Found operand types 'Edm.Int64' and 'Edm.String' for operator kind 'Equal'.");
        }

        [Fact]
        public async Task GetAliases_SortById_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "Id asc")).Items;

            AssertOrderIsValid("ons_", "Id", "asc", aliases);
        }

        [Fact]
        public async Task GetAliases_SortByIdDesc_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "Id desc")).Items;

            AssertOrderIsValid("ons_", "Id", "desc", aliases);
        }

        [Fact]
        public async Task GetAliases_SortByNamespace_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "Namespace asc"))
                .Items;

            AssertOrderIsValid("ons_", "Namespace", "asc", aliases);
        }

        [Fact]
        public async Task GetAliases_SortByNamespaceDesc_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "Namespace desc"))
                .Items;

            AssertOrderIsValid("ons_", "Namespace", "desc", aliases);
        }

        [Fact]
        public async Task GetAliases_SortByAlias_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "Alias asc")).Items;

            AssertOrderIsValid("ons_", "Alias", "asc", aliases);
        }

        [Fact]
        public async Task GetAliases_SortByAliasDesc_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "Alias desc")).Items;

            AssertOrderIsValid("ons_", "Alias", "desc", aliases);
        }

        [Fact]
        public async Task GetAliases_SortByDocumentId_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "DocumentId asc"))
                .Items;

            AssertOrderIsValid("ons_", "DocumentId", "asc", aliases);
        }

        [Fact]
        public async Task GetAliases_SortByDocumentIdDesc_ValidOrder()
        {
            await UseAdminAsync();
            var aliases = (await GetAliasesAsync(filter: "startswith(Namespace, 'ons_')", orderBy: "DocumentId desc"))
                .Items;

            AssertOrderIsValid("ons_", "DocumentId", "desc", aliases);
        }


        #endregion

        #region Helpers

        /// <summary>
        /// Asserts if document/published revision order is valid when
        /// OData clause $orderby=Accessed desc,Created desc is specified.
        /// First documents/revisions must be sorted by Accessed time.
        /// List may contain documents/revisions with null Accessed time in the end and
        /// these documents must be sorted by Created time
        /// </summary>
        /// <param name="actual"></param>
        private static void AssertOrderIsValid<T>(List<T> actual)
            where T : class
        {
            var expected = actual.GetRange(0, actual.Count)
                .OrderByDescending(
                    e => e, new Framework.Comparer<T>("Accessed"))
                .ThenByDescending(
                    e => e, new Framework.Comparer<T>("Created"))
                .ToList();

            Assert.Equal(
                Newtonsoft.Json.JsonConvert.SerializeObject(expected),
                Newtonsoft.Json.JsonConvert.SerializeObject(actual));
        }

        private void AssertOrderIsValid(string namespaceSearchText, string sortField, string order,
            List<AliasDto> actual)
        {
            var expected = _fixture.AllAliases.Where(alias => alias.Namespace.Contains(namespaceSearchText)).ToList();

            expected.Sort(new Comparer(sortField, order));

            Assert.Equal(
                Newtonsoft.Json.JsonConvert.SerializeObject(SelectField(expected, sortField)),
                Newtonsoft.Json.JsonConvert.SerializeObject(SelectField(actual, sortField)));
        }

        private static object SelectField(List<AliasDto> aliases, string field)
        {
            return field switch
            {
                "Id" => (object) aliases.Select(a => a.Id),
                "Namespace" => aliases.Select(a => a.Namespace),
                "Alias" => aliases.Select(a => a.Alias),
                "DocumentId" => aliases.Select(a => a.DocumentId),
                _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
            };
        }

        private class Comparer : IComparer<AliasDto>
        {
            private readonly string _sortField;
            private readonly string _order;

            public Comparer(string sortField, string order)
            {
                if (sortField != "Id" && sortField != "Namespace" && sortField != "Alias" && sortField != "DocumentId")
                    throw new ArgumentOutOfRangeException(nameof(sortField), sortField, null);
                if (order != "asc" && order != "desc")
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);

                _sortField = sortField;
                _order = order;
            }

            public int Compare(AliasDto x, AliasDto y)
            {
                if (x == null || y == null)
                    throw new ArgumentNullException();

                if (_order == "asc")
                    switch (_sortField)
                    {
                        case "Id":
                        case "DocumentId":
                        {
                            var xValue = (long) GetPropertyValue(x);
                            var yValue = (long) GetPropertyValue(y);
                            return xValue.CompareTo(yValue);
                        }
                        case "Namespace":
                        case "Alias":
                        {
                            var xValue = (string) GetPropertyValue(x);
                            var yValue = (string) GetPropertyValue(y);
                            return string.Compare(xValue, yValue, StringComparison.Ordinal);
                        }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(_sortField), _sortField, null);
                    }
                else
                    switch (_sortField)
                    {
                        case "Id":
                        case "DocumentId":
                        {
                            var xValue = (long) GetPropertyValue(x);
                            var yValue = (long) GetPropertyValue(y);
                            return yValue.CompareTo(xValue);
                        }
                        case "Namespace":
                        case "Alias":
                        {
                            var xValue = (string) GetPropertyValue(x);
                            var yValue = (string) GetPropertyValue(y);
                            return string.Compare(yValue, xValue, StringComparison.Ordinal);
                        }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(_sortField), _sortField, null);
                    }
            }

            private object GetPropertyValue(AliasDto src)
            {
                return src.GetType().GetProperty(_sortField)?.GetValue(src);
            }
        }

        #endregion
    }
}
