namespace Confirmit.Cdl.Api.Authorization
{
    public enum ResourceStatus
    {
        Exists,
        Archived
    }

    public enum Permission
    {
        None = 0,
        View = 1,
        Manage = 2
    }

    public enum Role
    {
        Administrator,
        NormalUser,
        Enduser
    }

    public enum UserType
    {
        User,
        Enduser
    }
}