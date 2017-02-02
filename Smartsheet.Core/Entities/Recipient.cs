using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class Recipient : SmartsheetObject
    {
        public Recipient()
        {

        }

        public Recipient(string email)
        {
            this.Email = email;
        }

        public string Email { get; set; }
        public long? GroupId { get; set; }
    }
}
