using ProfessionalServices.Core.Interfaces;

namespace Smartsheet.Core.Entities
{
    public class GroupMember : ISmartsheetObject
    {
        public long? Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string GroupName { get; set; }
    }
}
