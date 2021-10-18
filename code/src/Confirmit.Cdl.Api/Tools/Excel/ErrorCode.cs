namespace Confirmit.Cdl.Api.Tools.Excel
{
    public enum ErrorCode
    {
        MoreErrors = 0,
        EmptyId = 1,
        InvalidId = 2,
        DuplicateId = 3,
        NonexistentId = 4,
        EmptyPermission = 5,
        InvalidPermission = 6,
        PermissionManage = 7,
        TooManyNonexistentIds = 8
    }
}
