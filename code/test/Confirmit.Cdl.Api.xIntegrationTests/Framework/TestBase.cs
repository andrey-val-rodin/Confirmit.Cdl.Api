using ClosedXML.Excel;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.NetCore.IntegrationTestFramework;
using Confirmit.NetCore.IntegrationTestFramework.Fixtures;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    public class TestBase : CdlServiceClient
    {
        protected SharedFixture SharedFixture { get; }

        protected TestBase(SharedFixture fixture, ITestOutputHelper outputHelper)
        {
            SharedFixture = fixture;
            SharedFixture.TestName = outputHelper.GetTestName();
            Scope = fixture.CreateScope();
            Service = Scope.GetService<ICdl>();
        }

        protected async Task UseAdminAsync() => await SharedFixture.UseAdminAsync(Scope);
        protected async Task UseProsUserAsync() => await SharedFixture.UseProsUserAsync(Scope);
        protected async Task UseCompanyAdminAsync() => await SharedFixture.UseCompanyAdminAsync(Scope);
        protected async Task UseNormalUserAsync() => await SharedFixture.UseNormalUserAsync(Scope);
        protected async Task UseNormalUser2Async() => await SharedFixture.UseNormalUser2Async(Scope);
        protected async Task UseEnduserAsync() => await SharedFixture.UseEnduserAsync(Scope);
        protected void UseUnauthorizedUser() => SharedFixture.UseUnauthorizedUser(Scope);

        protected Company TestCompany => SharedFixture.TestCompany;
        protected Company TestCompany2 => SharedFixture.TestCompany2;
        protected User Admin => SharedFixture.Admin;
        protected User ProsUser => SharedFixture.ProsUser;
        protected User CompanyAdmin => SharedFixture.CompanyAdmin;
        protected User NormalUser => SharedFixture.NormalUser;
        protected User NormalUser2 => SharedFixture.NormalUser2;
        protected EnduserList EnduserList => SharedFixture.EnduserList;
        protected EnduserList EnduserList2 => SharedFixture.EnduserList2;
        protected Enduser Enduser => SharedFixture.Enduser;
        protected Enduser Enduser2 => SharedFixture.Enduser2;
        protected Enduser Enduser3 => SharedFixture.Enduser3;
        protected Enduser Enduser4 => SharedFixture.Enduser4;
        protected Enduser Enduser5 => SharedFixture.Enduser5;
        protected Enduser InactiveEnduser => SharedFixture.InactiveEnduser;
        protected Hub Hub1 => SharedFixture.Hub1;
        protected Hub Hub2 => SharedFixture.Hub2;
        protected Survey Survey1 => SharedFixture.Survey1;
        protected Survey Survey2 => SharedFixture.Survey2;

        protected static string GenerateRandomName()
        {
            return Guid.NewGuid().ToString();
        }

        protected static void AssertValidLinks(string expectedLinks, string actualLinks)
        {
            var teamCityLinks = expectedLinks
                .Replace("api/cdl", "")
                .Replace("//", "/");

            Assert.True(actualLinks == expectedLinks || actualLinks == teamCityLinks,
                $"Invalid links (expected and actual):\r\n{expectedLinks}\r\n{actualLinks}");
        }

        protected static Stream CreateExcel(string[][] rows)
        {
            var doc = new XLWorkbook();
            var sheet = doc.Worksheets.Add("Sheet1");

            var rowNumber = 1;
            foreach (var row in rows)
            {
                var colNumber = 1;
                foreach (var cell in row)
                {
                    if (!string.IsNullOrEmpty(cell))
                        sheet.Cell(rowNumber, colNumber).Value = cell;
                    colNumber++;
                }

                rowNumber++;
            }

            var memoryStream = new MemoryStream();
            doc.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
