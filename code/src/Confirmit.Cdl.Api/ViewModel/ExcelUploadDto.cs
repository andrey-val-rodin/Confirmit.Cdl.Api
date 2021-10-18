using Confirmit.Cdl.Api.Tools.Excel;
using JetBrains.Annotations;

namespace Confirmit.Cdl.Api.ViewModel
{
    [PublicAPI]
    public class ExcelUploadDto
    {
        public int UpdatedRecordsCount { get; set; }
        public int TotalErrorsCount { get; set; }
        public Error[] Errors { get; set; }
    }
}