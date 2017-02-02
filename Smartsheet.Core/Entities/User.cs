using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class User : ISmartsheetObject
    {
        public User()
        {

        }

        public long Id { get; set; }

        public int SheetCount { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }

        public bool Admin { get; set; }
        public bool LicensedSheetCreator { get; set; }
        public bool GroupAdmin { get; set; }
        public bool ResourceViewer { get; set; }

        public DateTime LastLogin { get; set; }
        public DateTime CustomWelcomeScreenViewed { get; set; }

        public ICollection<AlternateEmail> AlternateEmails { get; set; }
    }
}
