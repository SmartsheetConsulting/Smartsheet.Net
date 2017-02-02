using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class Email : SmartsheetObject
    {
        public Email()
        {

        }

        public string Subject { get; set; }
        public string Message { get; set; }
        public bool? CcMe { get; set; }

        public ICollection<Recipient> SendTo { get; set; }
    }
}
