using ClosedXML.Excel;
using Confirmit.Cdl.Api.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Confirmit.Cdl.Api.Tools.Excel
{
    public class Writer
    {
        private readonly IEnumerable<EnduserPermissionFullDto> _permissions;
        private IXLWorksheet _sheet;

        public Writer(IReadOnlyList<EnduserPermissionFullDto> permissions)
        {
            _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        }

        public Stream Write()
        {
            var doc = new XLWorkbook();
            _sheet = doc.Worksheets.Add("Sheet1");

            FillHeaders();
            FillData();

            var memoryStream = new MemoryStream();
            doc.SaveAs(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private void FillHeaders()
        {
            _sheet.Cell(1, 1).Value = "ID";
            _sheet.Cell(1, 2).Value = "Name";
            _sheet.Cell(1, 3).Value = "Full Name";
            _sheet.Cell(1, 4).Value = "Permission";
        }

        private void FillData()
        {
            int row = 2;
            foreach (var permission in _permissions)
            {
                _sheet.Cell(row, 1).Value = permission.Id;
                _sheet.Cell(row, 2).Value = permission.Name;
                _sheet.Cell(row, 3).Value = permission.FullName;
                _sheet.Cell(row, 4).Value = permission.Permission.ToString();

                row++;
            }
        }
    }
}
