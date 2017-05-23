using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smartsheet.Core.Entities
{
    public class Group : ISmartsheetObject
    {
        public Group() { }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public long OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public ICollection<GroupMember> Members { get; set; }
    }
}
