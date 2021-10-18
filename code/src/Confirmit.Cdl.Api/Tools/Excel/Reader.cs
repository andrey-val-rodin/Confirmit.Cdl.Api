using ClosedXML.Excel;
using Confirmit.Cdl.Api.Accounts;
using Confirmit.Cdl.Api.Authorization;
using Confirmit.Cdl.Api.Middleware;
using Confirmit.Cdl.Api.Tools.Validators;
using Confirmit.Cdl.Api.ViewModel;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools.Excel
{
    public class Reader
    {
        private IXLWorksheet _sheet;
        private readonly ErrorBag _errors = new ErrorBag();
        private readonly HashSet<int> _hash = new HashSet<int>();
        private readonly List<int> _rows = new List<int>();
        private readonly List<int> _ids = new List<int>();
        private readonly List<PermissionDto> _permissions = new List<PermissionDto>();
        private readonly EndusersValidator _validator;

        public Reader(IAccountLoader accountLoader)
        {
            _validator = new EndusersValidator(accountLoader);
        }

        public async Task<Tuple<IList<PermissionDto>, IList<Enduser>, ErrorBag>> Parse(Stream stream)
        {
            _sheet = GetWorksheet(stream);
            ParseRows();
            await _validator.ValidateAsync(_ids, ErrorBag.Threshold);
            if (_validator.CriticalErrorCountExceeded)
            {
                _errors.Reset();
                AddErrorsFromValidator(ErrorBag.Threshold - 1);
                _errors.AddError(ErrorCode.TooManyNonexistentIds);
                return new Tuple<IList<PermissionDto>, IList<Enduser>, ErrorBag>(
                    Enumerable.Empty<PermissionDto>().ToList(), Enumerable.Empty<Enduser>().ToList(), _errors);
            }

            if (_validator.WrongIds.Count > 0)
            {
                AddErrorsFromValidator();
                foreach (var id in _validator.WrongIds)
                {
                    var index = _ids.FindIndex(e => e == id);
                    _permissions.RemoveAt(index);
                }
            }

            return new Tuple<IList<PermissionDto>, IList<Enduser>, ErrorBag>(
                _permissions, _validator.ValidUsers, _errors);
        }

        private void AddErrorsFromValidator(int count = int.MaxValue)
        {
            foreach (var id in _validator.WrongIds)
            {
                count--;
                if (count < 0)
                    return;

                var index = _ids.FindIndex(e => e == id);
                var row = index >= 0 ? _rows[index] : 0;
                _errors.AddError(ErrorCode.NonexistentId, row, id.ToString());
            }
        }

        private static IXLWorksheet GetWorksheet(Stream stream)
        {
            try
            {
                var document = new XLWorkbook(stream);
                return document.Worksheets.FirstOrDefault();
            }
            catch (OpenXmlPackageException e)
            {
                if (e.ToString().Contains("Invalid Hyperlink"))
                {
                    UriFixer.FixInvalidUris(stream);
                    try
                    {
                        var document = new XLWorkbook(stream);
                        return document.Worksheets.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException("Input file is not a valid excel file: " + ex.Message, ex);
                    }
                }

                throw new BadRequestException("Input file is not a valid excel file: " + e.Message, e);
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Input file is not a valid excel file: " + ex.Message, ex);
            }
        }

        private void ParseRows()
        {
            int lastRow = _sheet.LastRowUsed().RowNumber();
            for (int row = 2; row <= lastRow; row++)
            {
                var permission = ParseRow(row);
                if (permission != null)
                {
                    _ids.Add(permission.Id);
                    _rows.Add(row);
                    _permissions.Add(permission);
                }
            }
        }

        private PermissionDto ParseRow(int rowNumber)
        {
            var row = _sheet.Row(rowNumber);
            if (row.IsEmpty())
                return null;

            var id = ParseId(rowNumber);
            return id > 0 && TryParsePermission(rowNumber, out var permission)
                ? new PermissionDto { Id = id, Permission = permission }
                : null;
        }

        private int ParseId(int rowNumber)
        {
            var row = _sheet.Row(rowNumber);
            var cell = row.Cell(1);
            if (cell.IsEmpty())
            {
                _errors.AddError(ErrorCode.EmptyId, rowNumber);
                return 0;
            }

            if (!cell.TryGetValue(out int result) || result <= 0)
            {
                _errors.AddError(ErrorCode.InvalidId, rowNumber, cell.GetString());
                return 0;
            }

            if (!CheckForDuplicates(rowNumber, result))
                return 0;

            return result;
        }

        private bool CheckForDuplicates(int rowNumber, int id)
        {
            if (_hash.Contains(id))
            {
                _errors.AddError(ErrorCode.DuplicateId, rowNumber, id.ToString());
                return false;
            }

            _hash.Add(id);
            return true;
        }

        private bool TryParsePermission(int rowNumber, out Permission permission)
        {
            permission = default;
            var row = _sheet.Row(rowNumber);
            var cell = row.Cell(4);
            if (cell.IsEmpty())
            {
                _errors.AddError(ErrorCode.EmptyPermission, rowNumber);
                return false;
            }

            var value = cell.GetString();
            if (!Enum.TryParse(value, true, out Permission result))
            {
                _errors.AddError(ErrorCode.InvalidPermission, rowNumber, null, value);
                return false;
            }

            if (result == Permission.Manage)
            {
                _errors.AddError(ErrorCode.PermissionManage, rowNumber, null, value);
                return false;
            }

            permission = result;
            return true;
        }
    }
}
