using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class RowEmail : Email
    {
        public RowEmail()
        {

        }

        public bool? IncludeAttachments { get; set; }
        public bool? IncludeDiscussions { get; set; }
        public string Layout { get; set; }

        public ICollection<long> ColumnIds { get; set; }
    }
}
