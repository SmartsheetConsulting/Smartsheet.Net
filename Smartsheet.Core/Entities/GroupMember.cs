using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smartsheet.Core.Entities
{
    public class GroupMember : ISmartsheetObject
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
    }
}
