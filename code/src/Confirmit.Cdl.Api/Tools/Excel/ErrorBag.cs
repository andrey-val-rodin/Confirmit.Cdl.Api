using System.Collections.Generic;
using System.Linq;

namespace Confirmit.Cdl.Api.Tools.Excel
{
    public class ErrorBag
    {
        public const int Threshold = 3;

        private static readonly Dictionary<ErrorCode, string> ErrorMessages =
            new Dictionary<ErrorCode, string>
            {
                { ErrorCode.MoreErrors, "<...More errors>" },
                { ErrorCode.EmptyId, "Row {0}: empty ID value" },
                { ErrorCode.InvalidId, "Row {0}: invalid ID value {1}" },
                { ErrorCode.DuplicateId, "Row {0}: duplicate ID value {1}" },
                { ErrorCode.NonexistentId, "Row {0}: enduser {1} does not exist or you do not have access to enduser list" },
                { ErrorCode.EmptyPermission, "Row {0}: empty permission value" },
                { ErrorCode.InvalidPermission, "Row {0}: invalid permission value {2}" },
                { ErrorCode.PermissionManage, "Row {0}: enduser cannot have permission Manage" },
                { ErrorCode.TooManyNonexistentIds, "Too many invalid IDs. Import aborted" }
            };

        private readonly List<Error> _errors = new List<Error>();

        public IReadOnlyCollection<Error> Errors => _errors;
        public int TotalCount { get; private set; }

        public void AddError(ErrorCode code, int rowNumber = 0, string id = null, string permission = null)
        {
            TotalCount++;

            // Check if error bag is full
            var lastError = _errors.LastOrDefault();
            if (lastError != null && lastError.Code == ErrorCode.MoreErrors)
                return;

            // Add error
            if (_errors.Count >= Threshold) // error bag is full
                _errors.Add(new Error
                {
                    Code = ErrorCode.MoreErrors,
                    Message = ErrorMessages[ErrorCode.MoreErrors]
                });
            else
                _errors.Add(CreateError(code, rowNumber, id, permission));
        }

        private static Error CreateError(ErrorCode code, int rowNumber, string id, string permission)
        {
            var message = string.Format(ErrorMessages[code], rowNumber, id, permission);
            return new Error
            {
                Code = code,
                Message = message
            };
        }

        public void Reset()
        {
            _errors.Clear();
        }
    }
}
