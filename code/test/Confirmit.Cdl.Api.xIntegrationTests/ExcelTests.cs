using ClosedXML.Excel;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Tools.Excel;
using Confirmit.Cdl.Api.ViewModel;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class ExcelTests : TestBase, IClassFixture<ExcelFixture>
    {
        private readonly ExcelFixture _fixture;

        public ExcelTests(SharedFixture sharedFixture,
            ExcelFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        #region DownloadEnduserPermissions

        [Fact]
        public async Task DownloadEnduserPermissions_Admin_ValidExcel()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            var stream = await DownloadEnduserPermissionsAsync(document.Id, Enduser.ListId);
            AssertValidExcel(stream,
                new[] { "ID", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "View" },
                new[] { $"{Enduser2.Id}", "Enduser2", "Enduser 2", "None" });
        }

        [Fact]
        public async Task DownloadEnduserPermissions_NormalUserWithoutAccessToEnduserList_NotFound()
        {
            await UseAdminAsync();
            await PatchUserPermissionsAsync(_fixture.DocumentId,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.Manage } });

            await UseNormalUserAsync();
            // NotFound expected because normal user has not permission to read enduser list
            await DownloadEnduserPermissionsAsync(_fixture.DocumentId, EnduserList.Id, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DownloadEnduserPermissions_Enduser_Forbidden()
        {
            await UseAdminAsync();
            await PatchEnduserPermissionsAsync(_fixture.DocumentId, new[]
                { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });

            await UseEnduserAsync();
            // NotFound expected because enduser has not permission to read permissions
            await DownloadEnduserPermissionsAsync(_fixture.DocumentId, EnduserList.Id, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task DownloadEnduserPermissions_WholeListPermission_AllEndusersHaveViewPermission()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            // Set whole list permission
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });

            var stream = await DownloadEnduserPermissionsAsync(document.Id, EnduserList.Id);
            // All users have "View" permission
            AssertValidExcel(stream,
                new[] { "ID", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "View" },
                new[] { $"{Enduser2.Id}", "Enduser2", "Enduser 2", "View" });
        }

        #endregion

        #region UploadEnduserPermissions

        [Fact]
        public async Task UploadEnduserPermissions_InvalidContent_BadRequest()
        {
            await UseAdminAsync();
            await UploadEnduserPermissionsAsync(_fixture.DocumentId, new MemoryStream(),
                HttpStatusCode.BadRequest, "Input file is not a valid excel file");
        }

        [Fact]
        public async Task UploadEnduserPermissions_ExcelWithWrongReferences_Ok()
        {
            await UseAdminAsync();
            await using var stream = new FileStream("./Data/WrongExternalRefs.xlsx", FileMode.Open, FileAccess.Read);
            await UploadEnduserPermissionsAsync(_fixture.DocumentId, stream);
        }

        [Fact]
        public async Task UploadEnduserPermissions_EmptyExcel_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 0,
                Errors = new Error[] { }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_ValidExcel_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "", "", "", "" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "View" },
                new[] { $"{Enduser2.Id}", "Enduser2", "Enduser 2", "View" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 2,
                TotalErrorsCount = 0,
                Errors = new Error[] { }
            };
            var expectedPermissions = new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View },
                new PermissionDto { Id = Enduser2.Id, Permission = Permission.View }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
            await CheckPermissionsAsync(document.Id, expectedPermissions);
        }

        [Fact]
        public async Task UploadEnduserPermissions_InvalidId_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "SomeText", "", "", "" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.InvalidId,
                        Message = "Row 2: invalid ID value SomeText"
                    }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_EmptyId_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "", "enduser", "", "" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.EmptyId,
                        Message = "Row 2: empty ID value"
                    }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_ZeroId_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "0", "Nonexistent", "Nonexistent Enduser", "View" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.InvalidId,
                        Message = "Row 2: invalid ID value 0"
                    }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_NonexistentId_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            const int wrongId = int.MaxValue;
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { wrongId.ToString(), "Nonexistent", "Nonexistent Enduser", "View" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.NonexistentId,
                        Message = $"Row 2: enduser {wrongId} does not exist or you do not have access to enduser list"
                    }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_DuplicateId_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "View" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "Manage" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 1,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.DuplicateId,
                        Message = $"Row 3: duplicate ID value {Enduser.Id}"
                    }
                }
            };
            var expectedPermissions = new[]
            {
                new PermissionDto { Id = Enduser.Id, Permission = Permission.View }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
            await CheckPermissionsAsync(document.Id, expectedPermissions);
        }

        [Fact]
        public async Task UploadEnduserPermissions_EmptyPermission_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.EmptyPermission,
                        Message = "Row 2: empty permission value"
                    }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_InvalidPermission_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "SomeText" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.InvalidPermission,
                        Message = "Row 2: invalid permission value SomeText"
                    }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_PermissionManage_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "Manage" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 1,
                Errors = new[]
                {
                    new Error
                    {
                        Code = ErrorCode.PermissionManage,
                        Message = "Row 2: enduser cannot have permission Manage"
                    }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_PermissionInCapitalLetters_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "VIEW" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 1,
                TotalErrorsCount = 0,
                Errors = new Error[] { }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_4Errors_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "", "enduser", "", "" },
                new[] { Enduser.Id.ToString(), "", "", "Manage" },
                new[] { "SomeText", "", "", "" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 3,
                Errors = new[]
                {
                    new Error { Code = ErrorCode.EmptyId, Message = "Row 2: empty ID value" },
                    new Error { Code = ErrorCode.PermissionManage, Message = "Row 3: enduser cannot have permission Manage" },
                    new Error { Code = ErrorCode.InvalidId, Message = "Row 4: invalid ID value SomeText" }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_ManyErrors_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "", "enduser", "", "" },
                new[] { "", "enduser", "", "" },
                new[] { "", "enduser", "", "" },
                new[] { "", "enduser", "", "" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 4,
                Errors = new[]
                {
                    new Error { Code = ErrorCode.EmptyId, Message = "Row 2: empty ID value" },
                    new Error { Code = ErrorCode.EmptyId, Message = "Row 3: empty ID value" },
                    new Error { Code = ErrorCode.EmptyId, Message = "Row 4: empty ID value" },
                    new Error { Code = ErrorCode.MoreErrors, Message = "<...More errors>" }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_CriticalErrors_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            const int wrongId1 = int.MaxValue;
            const int wrongId2 = wrongId1 - 1;
            const int wrongId3 = wrongId1 - 2;
            const int wrongId4 = wrongId1 - 3;
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { "", "enduser", "", "" },
                new[] { "", "enduser", "", "" },
                new[] { "", "enduser", "", "" },
                new[] { "", "enduser", "", "" },
                new[] { "", "enduser", "", "" },
                new[] { wrongId1.ToString(), "enduser", "", "View" },
                new[] { wrongId2.ToString(), "enduser", "", "View" },
                new[] { wrongId3.ToString(), "enduser", "", "View" },
                new[] { wrongId4.ToString(), "enduser", "", "View" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 0,
                TotalErrorsCount = 8,
                Errors = new[]
                {
                    new Error { Code = ErrorCode.NonexistentId, Message = $"Row 7: enduser {wrongId1} does not exist or you do not have access to enduser list" },
                    new Error { Code = ErrorCode.NonexistentId, Message = $"Row 8: enduser {wrongId2} does not exist or you do not have access to enduser list" },
                    new Error { Code = ErrorCode.TooManyNonexistentIds, Message = "Too many invalid IDs. Import aborted" }
                }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_ManyEndusersFromDifferentLists_CorrectResult()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { Enduser.Id.ToString(), Enduser.Name, Enduser.FullName, "View" },
                new[] { Enduser2.Id.ToString(), Enduser2.Name, Enduser2.FullName, "View" },
                new[] { Enduser3.Id.ToString(), Enduser3.Name, Enduser3.FullName, "View" },
                new[] { Enduser4.Id.ToString(), Enduser4.Name, Enduser4.FullName, "View" },
                new[] { Enduser5.Id.ToString(), Enduser5.Name, Enduser5.FullName, "View" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 5,
                TotalErrorsCount = 0,
                Errors = new Error[] { }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_PermissionNone_EnduserDeleted()
        {
            await UseAdminAsync();
            var document = await PostDocumentAsync();
            await PatchEnduserPermissionsAsync(document.Id,
                new[] { new PermissionDto { Id = Enduser.Id, Permission = Permission.View } });
            var page = await GetEnduserPermissionsAsync(document.Id, filter: $"Id eq {Enduser.Id}");

            Assert.Equal(1, page.TotalCount);
            Assert.Single(page.Items);
            Assert.Equal(Enduser.Id, page.Items[0].Id);
            Assert.Equal(Permission.View, page.Items[0].Permission);

            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "None" }
            });

            await UploadEnduserPermissionsAsync(document.Id, excel);
            page = await GetEnduserPermissionsAsync(document.Id, filter: $"Id eq {Enduser.Id}");

            Assert.Equal(0, page.TotalCount);
            Assert.Empty(page.Items);
        }

        [Fact]
        public async Task UploadEnduserPermissions_NormalUserWithoutPermissionsOnDocument_Forbidden()
        {
            await UseAdminAsync();

            await UseNormalUserAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "None" }
            });
            await UploadEnduserPermissionsAsync(_fixture.DocumentId, excel, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UploadEnduserPermissions_NormalUserWithPermissionViewOnDocument_Forbidden()
        {
            await UseAdminAsync();
            await PatchUserPermissionsAsync(_fixture.DocumentId,
                new[] { new UserPermissionDto { Id = NormalUser.Id, Permission = Permission.View } });

            await UseNormalUserAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "None" }
            });
            await UploadEnduserPermissionsAsync(_fixture.DocumentId, excel, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task UploadEnduserPermissions_ProsUserHasAccessToEnduserList_CorrectResult()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "None" }
            });

            var expectedResult = new ExcelUploadDto
            {
                UpdatedRecordsCount = 1,
                TotalErrorsCount = 0,
                Errors = new Error[] { }
            };
            var result = await UploadEnduserPermissionsAsync(document.Id, excel);

            AssertValidExcelUploadResponse(expectedResult, result);
        }

        [Fact]
        public async Task UploadEnduserPermissions_WholeListPermissions_NoWholeListPermissions()
        {
            await UseProsUserAsync();
            var document = await PostDocumentAsync();
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList.Id, Permission = Permission.View });
            await PutEnduserListPermissionAsync(document.Id,
                new PermissionDto { Id = EnduserList2.Id, Permission = Permission.View });

            var excel = CreateExcel(new[]
            {
                new[] { "Id", "Name", "Full Name", "Permission" },
                new[] { $"{Enduser.Id}", "Enduser", "Enduser 1", "View" },
                new[] { $"{Enduser3.Id}", "Enduser", "Enduser 1", "None" }
            });

            await UploadEnduserPermissionsAsync(document.Id, excel);

            var page = await GetEnduserListPermissionsAsync(document.Id);
            Assert.Equal(0, page.TotalCount);
            Assert.Empty(page.Items);
        }

        #endregion

        #region Helpers

        private static void AssertValidExcel(Stream stream, params string[][] rows)
        {
            Assert.NotNull(rows);
            Assert.True(rows.Length > 0);
            IXLWorksheet sheet = null;
            try
            {
                var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                var document = new XLWorkbook(memoryStream);
                sheet = document.Worksheets.FirstOrDefault();
            }
            catch (Exception)
            {
                Assert.True(false, "Unable to open Excel document");
            }

            Assert.NotNull(sheet);

            var rowCount = rows.Length;
            var colCount = rows[0].Length;
            Assert.Equal(sheet.LastRowUsed().RowNumber(), rowCount);
            Assert.Equal(sheet.LastColumnUsed().ColumnNumber(), colCount);

            var rowNumber = 1;
            foreach (var row in rows)
            {
                Assert.Equal(colCount, row.Length);

                var colNumber = 1;
                foreach (var cell in row)
                {
                    Assert.Equal(cell, sheet.Cell(rowNumber, colNumber).GetString());
                    colNumber++;
                }

                rowNumber++;
            }
        }

        private static void AssertValidExcelUploadResponse(ExcelUploadDto expected, ExcelUploadDto actual)
        {
            Assert.True(expected.UpdatedRecordsCount == actual.UpdatedRecordsCount, "Different UpdatedRecordsCount");
            Assert.True(expected.TotalErrorsCount == actual.TotalErrorsCount, "Different TotalErrorsCount");
            Assert.True(expected.Errors.Length == actual.Errors.Length, "Different number of errors");

            for (int i = 0; i < expected.Errors.Length; i++)
            {
                var expectedError = expected.Errors[i];
                var actualError = actual.Errors[i];

                Assert.True(expectedError.Code == actualError.Code, "Different errors");
                Assert.True(expectedError.Message == actualError.Message, "Different errors");
            }
        }

        private async Task CheckPermissionsAsync(long documentId, PermissionDto[] expected)
        {
            var page = await GetEnduserPermissionsAsync(documentId);

            Assert.Equal(expected.Length, page.TotalCount);
            Assert.Equal(expected.Length, page.Items.Count);

            foreach (var expectedPermission in expected)
            {
                var actualPermission = page.Items.FirstOrDefault(p => p.Id == expectedPermission.Id);

                Assert.NotNull(actualPermission);
                Assert.Equal(expectedPermission.Permission, actualPermission.Permission);
            }
        }

        #endregion
    }
}
