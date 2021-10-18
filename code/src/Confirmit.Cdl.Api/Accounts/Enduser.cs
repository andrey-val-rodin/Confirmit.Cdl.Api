namespace Confirmit.Cdl.Api.Accounts
{
    public class Enduser
    {
        public int Id { get; set; }
        public int ListId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
    }
}