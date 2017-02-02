using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class AlternateEmail : ISmartsheetObject
    {
        public AlternateEmail()
        {

        }

        public long Id { get; set; }

        public string Email { get; set; }

        public bool Confirmed { get; set; }
    }
}
