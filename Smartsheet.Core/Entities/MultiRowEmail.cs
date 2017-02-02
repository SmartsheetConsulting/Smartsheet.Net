using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class MultiRowEmail : RowEmail
    {
        public MultiRowEmail()
        {

        }

        public ICollection<long> RowIds { get; set; }
    }
}
